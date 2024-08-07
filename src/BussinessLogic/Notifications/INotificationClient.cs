namespace JourneyAPI.Notifications
{
    public interface INotificationClient
    {
        Task ReceiveNotification(string message);
    }
}
