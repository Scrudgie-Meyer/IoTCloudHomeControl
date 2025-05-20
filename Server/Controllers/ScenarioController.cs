using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data.DBManager;
using Server.Data.Entities;

[Route("api/[controller]")]
[ApiController]
public class ScenarioController : ControllerBase
{
    private readonly DBSetup _context;

    public ScenarioController(DBSetup context)
    {
        _context = context;
    }

    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleEvent([FromBody] ScheduledEventDto dto)
    {
        var device = await _context.Devices.FindAsync(dto.DeviceId);
        if (device == null)
            return NotFound("Device not found.");

        var scheduled = new ScheduledEvent
        {
            DeviceId = dto.DeviceId,
            EventType = dto.EventType,
            Description = dto.Description,
            ScheduledTime = DateTime.SpecifyKind(dto.ScheduledTime, DateTimeKind.Utc),
            IsRecurring = dto.IsRecurring,
            RecurrenceInterval = dto.RecurrenceInterval.HasValue ? dto.RecurrenceInterval : null,
            Device = device
        };

        _context.ScheduledEvents.Add(scheduled);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("user/{userId}/events")]
    public async Task<IActionResult> GetEventsForUser(int userId)
    {
        var events = await _context.ScheduledEvents
            .Include(e => e.Device)
            .Where(e => e.Device.UserId == userId)
            .Select(e => new
            {
                e.Id,
                e.DeviceId,
                DeviceName = e.Device.Name,
                e.EventType,
                e.Description,
                e.ScheduledTime,
                e.IsRecurring,
                e.RecurrenceInterval,
                e.IsEnabled,
                e.AudioFileName,
                e.AudioFilePath
            })
            .ToListAsync();

        return Ok(events);
    }

    [HttpGet("user/{userId}/device/{deviceSerialNumber}/events")]
    public async Task<IActionResult> GetEventsFoDevice(int userId, string deviceSerialNumber)
    {
        var eventsToSend = await _context.ScheduledEvents
            .Include(e => e.Device)
            .Where(e =>
                e.Device.UserId == userId &&
                e.Device.SerialNumber == deviceSerialNumber &&
                e.IsEnabled) // Only send enabled events
            .ToListAsync();

        // Prepare response model
        var response = eventsToSend.Select(e => new
        {
            e.Id,
            e.DeviceId,
            DeviceName = e.Device.Name,
            e.EventType,
            e.Description,
            e.ScheduledTime,
            e.IsRecurring,
            e.RecurrenceInterval,
            e.IsEnabled,
            e.AudioFileName,
            e.AudioFilePath
        }).ToList();

        // Update non-recurring events: disable after sending
        var nonRecurringEvents = eventsToSend
            .Where(e => !e.IsRecurring)
            .ToList();

        foreach (var evt in nonRecurringEvents)
        {
            evt.IsEnabled = false;
        }

        // Save changes only if needed
        if (nonRecurringEvents.Count > 0)
        {
            await _context.SaveChangesAsync();
        }

        return Ok(response);
    }


    [HttpDelete("event/{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var evt = await _context.ScheduledEvents.FindAsync(id);
        if (evt == null)
            return NotFound("Event not found.");

        _context.ScheduledEvents.Remove(evt);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Event deleted successfully", evt.Id });
    }


    [HttpPut("{id}/toggle")]
    public async Task<IActionResult> ToggleEvent(int id)
    {
        var evt = await _context.ScheduledEvents.FindAsync(id);
        if (evt == null)
            return NotFound();

        evt.IsEnabled = !evt.IsEnabled;
        await _context.SaveChangesAsync();

        return Ok(new { evt.Id, evt.IsEnabled });
    }

    // Update schedule
    [HttpPut("{id}/reschedule")]
    public async Task<IActionResult> RescheduleEvent(int id, [FromBody] DateTime newTime)
    {
        var evt = await _context.ScheduledEvents.FindAsync(id);
        if (evt == null)
            return NotFound();

        evt.ScheduledTime = newTime;
        await _context.SaveChangesAsync();

        return Ok(new { evt.Id, evt.ScheduledTime });
    }

    // Upload audio file
    [HttpPost("{id}/upload-audio")]
    public async Task<IActionResult> UploadAudio(int id, IFormFile file)
    {
        var evt = await _context.ScheduledEvents.FindAsync(id);
        if (evt == null || file == null || file.Length == 0)
            return BadRequest("Invalid request");

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "audio");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var fullPath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        evt.AudioFileName = fileName;
        evt.AudioFilePath = $"/uploads/audio/{fileName}"; // для клієнтів

        await _context.SaveChangesAsync();

        return Ok(new { evt.Id, evt.AudioFilePath });
    }


    // Download audio file
    [HttpGet("{id}/audio")]
    public async Task<IActionResult> GetAudio(int id)
    {
        var evt = await _context.ScheduledEvents.FindAsync(id);
        if (evt == null || string.IsNullOrEmpty(evt.AudioFilePath))
            return NotFound("Audio not found.");

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", evt.AudioFilePath.TrimStart('/'));
        if (!System.IO.File.Exists(filePath))
            return NotFound("Audio file does not exist on server.");

        var contentType = "audio/mpeg";
        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

        return File(fileBytes, contentType, evt.AudioFileName ?? "event-audio.mp3");
    }

}

public class ScheduledEventDto
{
    public int DeviceId { get; set; }
    public required string EventType { get; set; }
    public string? Description { get; set; }
    public DateTime ScheduledTime { get; set; }

    public bool IsRecurring { get; set; } = false;
    public TimeSpan? RecurrenceInterval { get; set; }
}

