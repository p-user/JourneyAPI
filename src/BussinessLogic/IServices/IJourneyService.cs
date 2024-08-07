using DataAccessLayer.IRepositories;
using SharedLayer;
using SharedLayer.Dtos;
using SharedLayer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.IServices
{
    public interface IJourneyService
    {
        Task<Guid> Add(JourneyDto jouneryDto, CancellationToken cancellationToken);
        Task<List<ViewJourneyDto>> GetAll(string userId, CancellationToken cancellationToken);
        Task<ViewJourneyDto> GetById(Guid id, CancellationToken cancellationToken);
        Task<GenericResponseDto> Delete(Guid id, CancellationToken cancellationToken);
        Task<Guid> UpdateJourney(Guid id, EditJourneyDto journey, CancellationToken cancellationToken);
        Task<ViewJourneyDto> ShareJourneyWithOtherUser(Guid journeyId, string userId, CancellationToken cancellationToken);
        Task<List<ViewJourneyDto>> GetFavorites(string userId, CancellationToken cancellationToken);
        Task<List<ViewJourneyDto>> GetMyFavorites(string userId, CancellationToken cancellationToken);
        Task<GenericResponseDto> SetAsFavorite(Guid id, string userName, bool isFavorite, CancellationToken cancellationToken);
        Task<List<ViewJourneyDto>> GetSharedWithme(string? userName, CancellationToken cancellationToken);
        Task<PagedResult<ViewJourneyDto>> FilterJourneys(JourneyFilterDto filter, PaginationDto pagination, CancellationToken cancellationToken);
        Task<string> GetSharingToken(Guid journeyId, CancellationToken cancellationToken);
        Task<PagedResult<MonthlyInsightsViewDto>> GetMonthlyInsights(DateOnly date, PaginationDto paginationDto, bool isDescending, CancellationToken cancellationToken);
    }
}
