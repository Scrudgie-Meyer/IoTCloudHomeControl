using System.ComponentModel.DataAnnotations;

namespace Server.Data.Entities
{
    public class ScheduledEvent
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }
        public Device Device { get; set; } = null!;

        [Required]
        public required string EventType { get; set; }

        public string? Description { get; set; }

        public DateTime ScheduledTime { get; set; }

        public bool IsExecuted { get; set; } = false;
        public bool IsEnabled { get; set; } = true;

        public string? AudioFileName { get; set; }
        public string? AudioFilePath { get; set; }

        public bool IsRecurring { get; set; } = false;
        public TimeSpan? RecurrenceInterval { get; set; }
    }

}
