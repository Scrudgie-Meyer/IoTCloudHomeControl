namespace Server.Data.Entities
{
    public class Device
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public required string Type { get; set; }
        public required string SerialNumber { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }

}
