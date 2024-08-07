using AutoMapper;
using BusinessLayer.IServices;
using BusinessLayer.Services;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedLayer;
using SharedLayer.Dtos;

namespace JourneyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles=GlobalConstants.AdminUser)]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMapper _mapper;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
        }

        [HttpGet("AllUsers")]
        [ProducesResponseType(typeof(List<ApplicationUserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(499)]
        public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
        {
            var users = _userManager.Users.ToList();
            var userDtos = users.Select(user => _mapper.Map<ApplicationUserDto>(user)).ToList();

            foreach (var userDto in userDtos)
            {
                var user = users.FirstOrDefault(u => u.Id == userDto.Id);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    
                    userDto.Role = roles.First();
                    

                }
            }
            var response = userDtos.Where(s => s.Status == Status.Enabled).ToList();
            return Ok(response);
        }


        [HttpPost("Suspend-Account/{userId}")]
        [ProducesResponseType(typeof(GenericResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(499)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SuspendUser(string userId, [FromQuery] DateTimeOffset lockoutEnd)
        {
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User NOT found!");
            }

           
            user.LockoutEnd = lockoutEnd; 
            user.LockoutEnabled = true;   

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var response  = new GenericResponseDto()
                {
                    Success = true,
                    Message= $"User with ID {userId} has been locked out until {lockoutEnd}."
                };
                return Ok(response);
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPost("Remove-Suspend-For-Account/{userId}")]
        [ProducesResponseType(typeof(GenericResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UnSuspend(string userId, [FromQuery] DateTimeOffset lockoutEnd)
        {

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User NOT found!");
            }


            user.LockoutEnd = null;
            user.LockoutEnabled = false;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var response = new GenericResponseDto()
                {
                    Success = true,
                    Message = $"Account has been unsuspended!"
                };
                return Ok(response);
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpDelete("RemoveUser/{userId}")]
        [ProducesResponseType(typeof(GenericResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User NOT found!");
            }


            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                var response = new GenericResponseDto()
                {
                    Success = true,
                    Message = $"User has been deleted!"
                };
                return Ok(response);
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }


    }
}
