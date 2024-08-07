using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.IRepositories
{
    public interface IJourneyShareRepository : IBaseRepository<JourneyShare>
    {
        Task<List<JourneyShare>> GetFavorites(string id, CancellationToken cancellationToken);
        Task<JourneyShare> GetByJourneyId(Guid journeyId, string userId, CancellationToken cancellationToken);
        Task<List<string>> GetUsersWithFavoriteJourney(Guid journeId, CancellationToken cancellationToken);
    }
}
