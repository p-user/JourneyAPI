using DataAccessLayer.Config;
using DataAccessLayer.Entities;
using DataAccessLayer.IRepositories;
using DataAccessLayer.QueryFilters;
using Microsoft.EntityFrameworkCore;
using SharedLayer;
using SharedLayer.Dtos;
using SharedLayer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataAccessLayer.Repositories
{
    public class JourneyRepository : BaseRepository<Journey>, IJourneyRepository
    {
        public JourneyRepository(ApiDbContext apiDbContext) : base(apiDbContext)
        {
        }

        public async Task<PagedResult<Journey>> FilterJourneys(JourneyFilterDto filter, PaginationDto pagination, CancellationToken cancellationToken)
        {
            var query = _entities.AsQueryable();

            query = query.ApplyFilterJourney(filter);
            var totalItems = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Journey>(items, totalItems, pagination.PageNumber, pagination.PageSize);


        }


        public async Task<Journey?> GetSharingJourney(Guid journeyId , CancellationToken cancellationToken)
        {
            return await _entities
                .Include(s=>s.SharedWith)
                .Where(s=>s.Id==journeyId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<Journey>> GetSharedWithUser(string id, CancellationToken cancellationToken) 
            => await _entities.Include(s=>s.SharedWith.Any(e=>e.SharedWithUserId==id)).ToListAsync(cancellationToken);

        public async Task<PagedResult<MonthlyInsightsViewDto>> GetGroupedJourneys(DateOnly date,int pageNumber, int pageSize, bool isDescending, CancellationToken cancellationToken)
        {
            var month = date.Month;
            var year = date.Year;
            var result = _context.ApplicationUsers
                .Include(s=> s.Journeys.Where(s => s.StartTime.Month == s.ArrivalTime.Month && s.StartTime.Month == month && s.StartTime.Year == year))
                //.GroupBy(s => s.UserId)
                .Select(group => new MonthlyInsightsViewDto
                {
                    UserId = group.Id,
                    Monthly = month,
                   // TotalDailyRewards = group.Where(s => s.DailyGoalAchieved).Count(),
                   // TotalDistance = group.Sum(s => s.RouteDistance),
                    TotalDailyRewards = group.Journeys.Where(s=>s.DailyGoalAchieved).Count(),
                    TotalDistance = group.Journeys.Sum(s=> s.RouteDistance),
                    TotalNoOfRoutes = group.Journeys.Count()
                });

            var orderedQuery = isDescending ? result.OrderByDescending(dto => dto.TotalDistance): result.OrderBy(dto => dto.TotalDistance);
            var totalCount = await result.CountAsync(cancellationToken);

            var pagedResults = await orderedQuery
                                .Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync(cancellationToken);

            return new PagedResult<MonthlyInsightsViewDto>
                ( items :  pagedResults,  totalItems: totalCount, pageNumber: pageNumber, pageSize: pageSize);


        }
    }
}
