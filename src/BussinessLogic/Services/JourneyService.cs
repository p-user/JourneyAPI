using AutoMapper;
using Azure.Core;
using BusinessLayer.IServices;
using DataAccessLayer.Config;
using DataAccessLayer.Entities;
using DataAccessLayer.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedLayer;
using SharedLayer.Dtos;
using SharedLayer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class JourneyService : IJourneyService
    {

        private readonly IJourneyRepository _journeyRepository;
        private readonly IJourneyShareRepository _journeyShareRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserContext _userContext;

        public JourneyService(IJourneyRepository journeyRepository, IMapper mapper, UserManager<ApplicationUser> userManager,   IUserContext userContext, IJourneyShareRepository journeyShareRepository)
        {
            _journeyRepository = journeyRepository;
            _mapper = mapper;
            _userManager = userManager;
            _userContext = userContext;
            _journeyShareRepository=journeyShareRepository;
        }

        public async Task<Guid> Add(JourneyDto journeyDto, CancellationToken cancellationToken)
        {
            try
            {
                var mappedEntity = _mapper.Map<Journey>(journeyDto);
                mappedEntity.UserId = _userContext.GetUserId();
                var journey = await _journeyRepository.AddAsync(mappedEntity, cancellationToken);
                await _journeyRepository.SaveChangesAsync(cancellationToken);
                return journey.Id;
            }
            catch (Exception ex) { throw; }
           
        }

        public async Task<List<ViewJourneyDto>> GetAll(string userId, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(userId);
                if (user is null)
                {
                    throw new ArgumentException("Invalid user ID!");
                }
                var isAdmin = await _userManager.IsInRoleAsync(user, GlobalConstants.AdminUser);
                List<Journey> entities;
                if (isAdmin)
                {
                     entities = await _journeyRepository.GetAllAsync(cancellationToken);
                }
                else 
                {
                    entities = await _journeyRepository.GetAllAsync(cancellationToken,j => j.UserId == userId); 
                }
                return _mapper.Map<List<ViewJourneyDto>>(entities);
            }
            catch(Exception ex) { throw; }  
        }
    

        public async Task<ViewJourneyDto> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _journeyRepository.GetAsync(id, cancellationToken);
                if (entity is null)
                {
                    throw new Exception("Entity not Found!");
                }
                var response = _mapper.Map<ViewJourneyDto>(entity);
                return response;
            }
            catch (Exception ex) { throw; }
           
        }

        public async Task<GenericResponseDto> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var journey = await _journeyRepository.GetAsync(id, cancellationToken);
                if (journey is not null)
                {
                    _journeyRepository.Delete(journey);
                    await _journeyRepository.SaveChangesAsync(cancellationToken);
                    return new GenericResponseDto()
                    {
                        Success = true,
                        Message = "Entity Deleted successfully!"
                    };

                }
                return new GenericResponseDto()
                {
                    Success = false,
                    Message = "Entity Not found!"
                };
            }
            catch (Exception e) { throw; }
           

        }

        public async Task<Guid> UpdateJourney(Guid id, EditJourneyDto journey, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _journeyRepository.GetAsync(id, cancellationToken);
                if (entity is null)
                {
                    throw new Exception("Entity not found!");
                }
                _mapper.Map( journey, entity);
                _journeyRepository.UpdateAsync(entity);
                await _journeyRepository.SaveChangesAsync(cancellationToken);
                return entity.Id;

            }
            catch (Exception e) { throw; }
        }


        public async Task<ViewJourneyDto> ShareJourneyWithOtherUser(Guid journeyId, string userId , CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                {
                    throw new Exception("User not found!");
                }
                var journey = await _journeyRepository.GetSharingJourney(journeyId, cancellationToken);

                if (journey is null)
                {
                    throw new Exception("Journey not found or deleted already");
                }
                ValidateSharingJourney(userId, journey);

                journey.SharedWith.Add(new JourneyShare
                {
                    JourneyId = journeyId,
                    SharedWithUser = user,
                });

                _journeyRepository.UpdateAsync(journey);
                await _journeyRepository.SaveChangesAsync(cancellationToken);

                var savedEntity = await _journeyRepository.GetSharingJourney(journeyId, cancellationToken);
                var result = _mapper.Map<ViewJourneyDto>(savedEntity);
                return result;
            }
            catch (Exception ex) { throw; }
            



        }

        private void ValidateSharingJourney(string userId, Journey journey)
        {
            
            if (journey.SharedWith.Any(s => s.SharedWithUserId == userId))
            {
                throw new Exception("Journey is already shared with this user! Please, provide another user!");
            }

            if (journey.UserId == userId)
            {
                throw new Exception("You can NOT share the journey with yourself! Be reasonable!");
            }

           
        }

        public async Task<List<ViewJourneyDto>> GetFavorites(string userId, CancellationToken cancellationToken)
        {
            try
            {
                var entities = await _journeyShareRepository.GetFavorites(userId, cancellationToken);
                return _mapper.Map<List<ViewJourneyDto>>(entities);

            }
            catch (Exception ex) { throw; }

        }

        public async Task<List<ViewJourneyDto>> GetMyFavorites(string userId, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(userId);
                if (user is null)
                {
                    throw new ArgumentException("Invalid user ID!");
                }

                var entities = await _journeyShareRepository.GetFavorites(user.Id, cancellationToken);
                return _mapper.Map<List<ViewJourneyDto>>(entities);

            }
            catch (Exception ex) { throw; }
        }

        public async  Task<GenericResponseDto> SetAsFavorite(Guid id, string userName, bool isFavorite, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (user is null)
                {
                    throw new ArgumentException("Could not retrive user details!");
                }
                var journey = await _journeyShareRepository.GetByJourneyId(id, user.Id, cancellationToken);
                if (journey is null)
                {
                    throw new Exception("No shared Journey was found!");
                }
                journey.IsFavorite = isFavorite;
                _journeyShareRepository.UpdateAsync(journey);
                await _journeyShareRepository.SaveChangesAsync(cancellationToken);
                return new GenericResponseDto()
                {
                    Success= true,
                    Message = "Journey updated!"
                };

            }
            catch (Exception ex) { throw; }
        }

        public async Task<List<ViewJourneyDto>> GetSharedWithme(string? userName, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (user is null)
                {
                    throw new ArgumentException("Invalid user ID!");
                }

                var entities = await _journeyRepository.GetSharedWithUser(user.Id, cancellationToken);
                return _mapper.Map<List<ViewJourneyDto>>(entities);

            }
            catch (Exception ex) { throw; }
        }

        public async Task<PagedResult<ViewJourneyDto>> FilterJourneys(JourneyFilterDto filter, PaginationDto pagination, CancellationToken cancellationToken)
        {
            try
            {

                var entities = await _journeyRepository.FilterJourneys(filter, pagination, cancellationToken);
                var result = new PagedResult<ViewJourneyDto>
                    (
                        _mapper.Map<List<ViewJourneyDto>>(entities.Items),
                        entities.TotalItems,
                        pagination.PageNumber,
                        pagination.PageSize

                     );
                return result;
            }
            catch (Exception ex) { throw; }

        }

        public async Task<string> GetSharingToken(Guid journeyId, CancellationToken cancellationToken)
        {
            try
            {
                var journey = await _journeyRepository.GetAsync(journeyId, cancellationToken);
                if (journey is null)
                {
                    throw new Exception("Entity Not found");
                }
                string result = null;
                if (string.IsNullOrEmpty(journey.SocialMediaLink))
                {
                    journey.SocialMediaLink = Guid.NewGuid().ToString();
                    result = journey.SocialMediaLink;
                    _journeyRepository.UpdateAsync(journey);
                    await _journeyRepository.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    result =journey.SocialMediaLink;
                }
                return result;

            }
            catch (Exception ex) { throw; }
        }

        public async Task<PagedResult<MonthlyInsightsViewDto>> GetMonthlyInsights(DateOnly date,PaginationDto paginationDto,bool isDescending, CancellationToken cancellationToken)
        {
            try
            {
                var journeys = await _journeyRepository.GetGroupedJourneys(date, paginationDto.PageNumber, paginationDto.PageSize, isDescending, cancellationToken);
                return journeys;

            }
            catch (Exception ex) { throw; }
        }
    }
}
