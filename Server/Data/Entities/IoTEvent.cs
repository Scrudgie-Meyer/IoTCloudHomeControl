using System.ComponentModel.DataAnnotations;

namespace Server.Data.Entities
{
    public class IoTEvent
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }
        public Device Device { get; set; } = null!;

        [Required]
        public required string EventType { get; set; }

        public string? Description { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
