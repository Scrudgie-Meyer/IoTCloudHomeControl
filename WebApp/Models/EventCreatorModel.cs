namespace WebApp.Models
{
    public class EventCreatorModel
    {
        public int DeviceId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ScheduledTime { get; set; }

        public bool IsRecurring { get; set; } = false;
        public int? RecurrenceHours { get; set; }
    }
}
