namespace Server.Services
{

    public class ScheduledEventService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ScheduledEventService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    using var scope = _serviceProvider.CreateScope();
            //    var db = scope.ServiceProvider.GetRequiredService<DBSetup>();

            //    var dueEvents = await db.ScheduledEvents
            //        .Include(e => e.Device)
            //        .Where(e => e.ScheduledTime <= DateTime.UtcNow)
            //        .ToListAsync(stoppingToken);

            //    foreach (var scheduled in dueEvents)
            //    {
            //        if (!scheduled.IsExecuted || scheduled.IsRecurring)
            //        {
            //            db.IoTEvents.Add(new IoTEvent
            //            {
            //                DeviceId = scheduled.DeviceId,
            //                EventType = scheduled.EventType,
            //                Description = scheduled.Description,
            //                Timestamp = scheduled.ScheduledTime,
            //                Device = scheduled.Device
            //            });

            //            if (scheduled.IsRecurring && scheduled.RecurrenceInterval.HasValue)
            //            {
            //                scheduled.ScheduledTime = scheduled.ScheduledTime.Add(scheduled.RecurrenceInterval.Value);
            //                scheduled.IsExecuted = false;
            //            }
            //            else
            //            {
            //                scheduled.IsExecuted = true;
            //            }
            //        }
            //    }

            //    await db.SaveChangesAsync(stoppingToken);
            //    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            //}
        }
    }
}
