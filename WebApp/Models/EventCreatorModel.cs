namespace WebApp.Models
{
    public class EventCreatorModel
    {
        public int DeviceId { get; set; }

        public string EventType { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime ScheduledTime { get; set; }

        public bool IsRecurring { get; set; } = false;

        public TimeSpan? RecurrenceInterval { get; set; }
    }



    public class DeviceOptionModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentDeviceId { get; set; } = null;
    }
}
