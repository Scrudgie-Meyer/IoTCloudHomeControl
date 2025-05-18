using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data.DBManager;
using Server.Data.Entities;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly DBSetup _context;

        public DeviceController(DBSetup context)
        {
            _context = context;
        }

        // 📌 Додати новий девайс
        [HttpPost("add")]
        public async Task<IActionResult> AddDevice([FromBody] DeviceDto dto)
        {
            var device = new Device
            {
                Name = dto.Name,
                Type = dto.Type,
                SerialNumber = dto.SerialNumber,
                UserId = dto.UserId,
                ScheduledTime = dto.ScheduledTime
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            return Ok(device);
        }

        // ❌ Видалити девайс і всі залежності
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var device = await _context.Devices
                .Include(d => d.ScheduledEvents)
                .Include(d => d.TriggeredEvents)
                .Include(d => d.IoTEvents)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
                return NotFound("Device not found");

            // Видаляємо всі пов'язані записи
            _context.ScheduledEvents.RemoveRange(device.ScheduledEvents);
            _context.TriggeredEvents.RemoveRange(device.TriggeredEvents);
            _context.IoTEvents.RemoveRange(device.IoTEvents);

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            return Ok("Device and related events deleted");
        }

        // 🔍 Отримати девайс за UserId та SerialNumber
        [HttpGet("user/{userId}/serial/{serialNumber}")]
        public async Task<IActionResult> GetDeviceByUserIdAndSerial(int userId, string serialNumber)
        {
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.UserId == userId && d.SerialNumber == serialNumber);

            if (device == null)
                return NotFound("Device not found");

            return Ok(device);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetDevicesByUserId(int userId)
        {
            var devices = await _context.Devices
                .Where(d => d.UserId == userId)
                .Select(d => new UserDeviceDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Type = d.Type,
                    ParentDeviceId = d.ParentDeviceId
                })
                .ToListAsync();

            return Ok(devices);
        }



    }

    public class DeviceDto
    {
        public string? Name { get; set; }
        public required string Type { get; set; }
        public required string SerialNumber { get; set; }
        public DateTime ScheduledTime { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }
    }

    public class UserDeviceDto
    {
        public int Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int? ParentDeviceId { get; set; }
    }
}
