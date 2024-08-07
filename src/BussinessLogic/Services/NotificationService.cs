using BusinessLayer.IServices;
using DataAccessLayer.Entities;
using DataAccessLayer.IRepositories;
using JourneyAPI.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SharedLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationsHub, INotificationClient> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJourneyShareRepository _journeyShareRepository;

        public NotificationService(IHubContext<NotificationsHub, INotificationClient> hubContext, UserManager<ApplicationUser> userManager, IJourneyShareRepository journeyShareRepository)
        {
            _hubContext = hubContext;
            _userManager = userManager;
            _journeyShareRepository = journeyShareRepository;

        }

        public async Task SendAdminNotification(string action, Guid journeyId, CancellationToken cancellationToken)
        {
            var admins = await _userManager.GetUsersInRoleAsync(GlobalConstants.AdminUser);
            var adminList = admins.Select(s => s.Id).ToList();

            var message = FormatMessage(action, journeyId);
            await SendNotification(message, adminList);
        }

        public async Task SendNotification(string action, Guid journeyId, CancellationToken cancellationToken)
        {
            //get all users that have this journey as favorite
            var usersList = await _journeyShareRepository.GetUsersWithFavoriteJourney(journeyId, cancellationToken);
            var admins = await _userManager.GetUsersInRoleAsync(GlobalConstants.AdminUser);
            var adminList = admins.Select(s=>s.Id).ToList();
            var notificationToSend = usersList.Concat(adminList).Distinct().ToList();

            var message = FormatMessage(action, journeyId);
            await SendNotification(message, notificationToSend);

        }
        public async Task SendNotification(string message, List<string> users)
        {
            foreach (var user in users)
            {
                await _hubContext.Clients.User(user).ReceiveNotification(message);
            }
        }

        private string FormatMessage(string action, Guid journeyId )
        {

            return $"Journey {journeyId} has been {action}";
        }
    }
}
 