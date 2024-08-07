using DataAccessLayer.Entities;
using SharedLayer;
using SharedLayer.Dtos;
using SharedLayer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.IRepositories
{
    public interface IJourneyRepository : IBaseRepository<Journey>
    {
        Task<PagedResult<Journey>> FilterJourneys(JourneyFilterDto filter, PaginationDto pagination, CancellationToken cancellationToken);
        Task<Journey?> GetSharingJourney(Guid journeyId, CancellationToken cancellationToken);
        Task<List<Journey>> GetSharedWithUser(string id, CancellationToken cancellationToken);
        Task<PagedResult<MonthlyInsightsViewDto>> GetGroupedJourneys(DateOnly date, int pageNumber, int pageSize, bool isDescending, CancellationToken cancellationToken);
    }
}
