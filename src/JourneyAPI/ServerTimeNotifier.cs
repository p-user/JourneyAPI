using JourneyAPI.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace JourneyAPI
{
    public class ServerTimeNotifier : BackgroundService
    {
        private static readonly TimeSpan Period = TimeSpan.FromSeconds(5);
        private readonly ILogger<ServerTimeNotifier> _logger;
        private readonly IHubContext<NotificationsHub, INotificationClient> _context;

        public ServerTimeNotifier(ILogger<ServerTimeNotifier> logger, IHubContext<NotificationsHub, INotificationClient> context)
        {
            _logger = logger;
            _context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            using var timer = new PeriodicTimer(Period);

            while (!cancellation.IsCancellationRequested &&  await timer.WaitForNextTickAsync(cancellation))
            {
                var dateTime = DateTime.Now;

                _logger.LogInformation("Executing {Service} {Time}", nameof(ServerTimeNotifier), dateTime);

                await _context.Clients.All.ReceiveNotification($"Server time = {dateTime}");
            }
        }
    }
}
