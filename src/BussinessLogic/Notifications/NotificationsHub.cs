using Microsoft.AspNetCore.SignalR;

namespace JourneyAPI.Notifications
{
    public class NotificationsHub : Hub<INotificationClient>
    {
    }
}
