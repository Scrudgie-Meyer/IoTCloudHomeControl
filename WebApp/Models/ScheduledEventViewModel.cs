namespace WebApp.Models
{
    public class ScheduledEventViewModel
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public required string EventType { get; set; }
        public string? Description { get; set; }
        public DateTime ScheduledTime { get; set; }
        public bool IsRecurring { get; set; }
        public TimeSpan? RecurrenceInterval { get; set; }
        public bool IsEnabled { get; set; }
        public string? AudioFileName { get; set; }
        public string? AudioFilePath { get; set; }
    }

}
