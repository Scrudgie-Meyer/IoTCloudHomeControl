using Microsoft.AspNetCore.Mvc;
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
            ScheduledTime = dto.ScheduledTime,
            IsRecurring = dto.IsRecurring,
            RecurrenceInterval = dto.RecurrenceHours.HasValue ? TimeSpan.FromHours(dto.RecurrenceHours.Value) : null,
            Device = device
        };

        _context.ScheduledEvents.Add(scheduled);
        await _context.SaveChangesAsync();

        return Ok(scheduled);
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

    // 🕒 Update schedule
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

    // 🔊 Upload audio file
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


    // 🔊 Download audio file
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
    public int? RecurrenceHours { get; set; } // For example: 6 = every 6 hours
}

