using SharedLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.IServices
{
    public interface INotificationService
    {
        Task SendNotification(string action, Guid journeyId, CancellationToken cancellationToken);
        Task SendAdminNotification(string action, Guid journeyId, CancellationToken cancellationToken);
    }
}
