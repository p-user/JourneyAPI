using BusinessLayer.IServices;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedLayer;
using SharedLayer.Dtos;
using SharedLayer.Utilities;
using System;
using System.Text.Json;

namespace JourneyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JourneyController : ControllerBase
    {
        private readonly IJourneyService _journeyService;
        private readonly INotificationService _NotificationService;

        public JourneyController(IJourneyService journeyService, INotificationService notificationService)
        {
            _journeyService = journeyService;
            _NotificationService = notificationService;
        }

        /// <summary>
        /// Retrieve a specific journey
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ViewJourneyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(499)]
        [HttpGet("Journey/{id}")]
        [Authorize]
        public async Task<IActionResult> GetJourney(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var results = await _journeyService.GetById(id, cancellationToken);
                return Ok(results);

            }
            catch (Exception ex) { return NotFound(); }
        }


        /// <summary>
        /// Retrive journeys of the user, if its Admin, will get all journeys of all users
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("AllJourney")]
        [ProducesResponseType(typeof(List<JourneyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(499)]
        [Authorize]
        public async Task<IActionResult> GetJourneys(CancellationToken cancellationToken)
        {
            var userId = User.Identity.Name;

            if (userId is not null)
            {
                var results = await _journeyService.GetAll(userId, cancellationToken);
                return Ok(results);
            }
            return Unauthorized();
        }
        /// <summary>
        /// Create Journey
        /// </summary>
        /// <param name="journeyDto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("Add")]
        [ProducesResponseType(typeof(ViewJourneyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ViewJourneyDto), StatusCodes.Status201Created)]
        [ProducesResponseType(499)]
        [Authorize]
        public async Task<IActionResult> AddJourney(JourneyDto journeyDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Please, provide data in a correct format!");
            }

            var guid = await _journeyService.Add(journeyDto, cancellationToken);
            await _NotificationService.SendAdminNotification(ActionConstants.Created, guid, cancellationToken);
            var result = await _journeyService.GetById(guid, cancellationToken);
            return Ok(result);
        }


        /// <summary>
        /// Delete journey
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("Delete")]
        [ProducesResponseType(typeof(GenericResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(499)]
        [Authorize]
        public async Task<IActionResult> DeleteJourney(Guid guid, CancellationToken cancellationToken)
        {
            var response = await _journeyService.Delete(guid, cancellationToken);
            await _NotificationService.SendAdminNotification(ActionConstants.Deleted, guid, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Updateing my jopunry
        /// </summary>
        /// <param name="id"></param>
        /// <param name="journey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("Update/{id}")]
        [ProducesResponseType(typeof(ViewJourneyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(499)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<ActionResult> UpdateJourney(Guid id, [FromBody] EditJourneyDto journey, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Please, provide data in a correct format!");
            }
            var response = await _journeyService.UpdateJourney(id, journey, cancellationToken);
            await _NotificationService.SendNotification(ActionConstants.Updated, id, cancellationToken);
            var result = await _journeyService.GetById(response, cancellationToken);
            return Ok(result);
        }


        /// <summary>
        /// Share journey with some other user
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("ShareMyJourneyWithOtherUser")]
        [ProducesResponseType(typeof(ViewJourneyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(499)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<ActionResult> ShareJourney([FromBody] ShareJourneyRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Please, provide data in a correct format!");
            }

            var result = await _journeyService.ShareJourneyWithOtherUser(request.JourneyId, request.userId, cancellationToken);
            return Ok(result);
        }



        /// <summary>
        /// Get a list of current users' favorite journeys
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("MyFavoriteJourneys")]
        [ProducesResponseType(typeof(List<ViewJourneyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(499)]
        [Authorize]
        public async Task<ActionResult> GetFavouriteJourneysOfCurrentUser(CancellationToken cancellationToken)

        {
            var userId = User.Identity.Name;

            if (userId is not null)
            {

                var result = await _journeyService.GetMyFavorites(userId, cancellationToken);
                return Ok(result);
            }
            return BadRequest();
        }


        /// <summary>
        /// Set/unset a journey as favorite
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isFavorite"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("{id}/favorite")]
        [ProducesResponseType(typeof(GenericResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(499)]
        [Authorize]
        public async Task<IActionResult> SetFavorite(Guid id, bool isFavorite, CancellationToken cancellationToken)
        {
            var userName = User.Identity.Name;
            var result = await _journeyService.SetAsFavorite(id, userName, isFavorite, cancellationToken);
            return Ok(result);
        }


        /// <summary>
        /// Get all journeys shared with me
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("journeys-shared-with-me")]
        [ProducesResponseType(typeof(List<ViewJourneyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(499)]
        [Authorize]
        public async Task<IActionResult> GetSharedWithMe(CancellationToken cancellationToken)
        {
            var userName = User.Identity.Name;
            var result = await _journeyService.GetSharedWithme(userName, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Share token by its token
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}/share-url")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(499)]
        [Authorize]
        public async Task<IActionResult> GetSharingToken([FromRoute]Guid id, CancellationToken cancellationToken)
        {
            var result = await _journeyService.GetSharingToken(id, cancellationToken);
            var shareableUrl = $"{Request.Scheme}://{Request.Host}/api/journeys/shared/{result}";

            return Ok(new { Url = shareableUrl });
           
        }




        #region Admin restricted area


        /// <summary>
        /// Dynamic filtering for all jounerys
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("Filter-Journeys")]
        [ProducesResponseType(typeof(PagedResult<ViewJourneyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(499)]
        [Authorize]
        public async Task<IActionResult> FilterJourneys([FromQuery] PaginationDto pagination, [FromBody] JourneyFilterDto filter, CancellationToken cancellationToken)
        {
            
            var result = await _journeyService.FilterJourneys(filter, pagination, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get favorite journeys of some user , providing the id of user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("FavoriteJourneys/{userId}")]
        [ProducesResponseType(typeof(List<ViewJourneyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(499)]
        [Authorize(Roles = GlobalConstants.AdminUser)]
        public async Task<ActionResult> GetFavouriteJourneysOfUser([FromRoute] string userId, CancellationToken cancellationToken)
        {

            var result = await _journeyService.GetFavorites(userId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Provide insights in monthly basis
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("MonthlyInsights")]
        [ProducesResponseType(typeof(List<MonthlyInsightsViewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(499)]
        [Authorize(Roles = GlobalConstants.AdminUser)]
        public async Task<ActionResult> GetMonthlyInsights([FromQuery] DateOnly date, [FromQuery] PaginationDto paginationDto, [FromQuery] bool isDescending, CancellationToken cancellationToken)
        {

            var result = await _journeyService.GetMonthlyInsights(date, paginationDto, isDescending, cancellationToken);
            return Ok(result);
        }
        #endregion


    }
}
