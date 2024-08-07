using DataAccessLayer.Config;
using DataAccessLayer.Entities;
using DataAccessLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class JourneyShareRepository : BaseRepository<JourneyShare>, IJourneyShareRepository
    {
        public JourneyShareRepository(ApiDbContext apiDbContext) : base(apiDbContext)
        {
        }
        public async Task<List<JourneyShare>> GetFavorites(string id, CancellationToken cancellationToken) 
            => await _entities.Where(s => s.SharedWithUserId == id && s.IsFavorite==true).ToListAsync(cancellationToken);

        public async Task<JourneyShare> GetByJourneyId(Guid journeyId, string userId, CancellationToken cancellationToken)
            => await _entities.Where(s => s.JourneyId==journeyId && s.SharedWithUserId == userId).FirstOrDefaultAsync(cancellationToken);

        public async Task<List<string>> GetUsersWithFavoriteJourney (Guid journeId, CancellationToken cancellationToken)
        {
            var result =  await _entities.Where(s=>s.JourneyId == journeId && s.IsFavorite== true).Select(s=>s.SharedWithUserId).ToListAsync(cancellationToken);    
            return result;
        }

    }
}
