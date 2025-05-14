using System.ComponentModel.DataAnnotations;

namespace Server.Data.Entities
{
    public class TriggeredEvent
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }
        public Device Device { get; set; } = null!;

        [Required]
        public required string TriggerType { get; set; } // наприклад: "Motion", "Temperature", "DoorOpened"

        public string? Description { get; set; }

        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

        public string? SensorValue { get; set; } // опціонально: зберегти значення, наприклад температура або ID події
    }
}
