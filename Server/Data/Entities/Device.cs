using System.ComponentModel.DataAnnotations;

namespace Server.Data.Entities
{
    public class Device
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        [Required]
        public required string Type { get; set; }

        [Required]
        public required string SerialNumber { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public DateTime ScheduledTime { get; set; }

        public bool IsActive { get; set; } = true;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public ICollection<IoTEvent> IoTEvents { get; set; } = new List<IoTEvent>();
        public ICollection<ScheduledEvent> ScheduledEvents { get; set; } = new List<ScheduledEvent>();
        public ICollection<TriggeredEvent> TriggeredEvents { get; set; } = new List<TriggeredEvent>();
    }


}
