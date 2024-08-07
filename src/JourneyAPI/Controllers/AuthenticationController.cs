using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using BusinessLayer.IServices;
using BusinessLayer.Services;
using Microsoft.AspNetCore.Authorization;
using SharedLayer;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Abstractions;
using SharedLayer.Dtos;

namespace JourneyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationController(UserManager<ApplicationUser> userManager, ITokenService tokenService, SignInManager<ApplicationUser> signInManager, IEmailService emailService, IUrlHelperFactory urlHelper, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _emailService = emailService;
            _urlHelperFactory = urlHelper;
            _httpContextAccessor = httpContextAccessor;

        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto user)
        {

            if (ModelState.IsValid)
            {
                var userExists = await _userManager.FindByEmailAsync(user.Email);
                if (userExists is not null || userExists.Status == Status.Enabled)
                {
                    return BadRequest("You must login ! The user is already registered!");
                }

                var newUser = new ApplicationUser
                {
                    Email = user.Email,
                    UserName = user.Email

                };

                var isCreated = await _userManager.CreateAsync(newUser, user.Password);

                if (isCreated.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, GlobalConstants.RoleUser);
                    await SendConfirmationEmail(newUser.Email, newUser);

                    return Ok("Please, Confirm your email account!");
                }

                return BadRequest("Error in registrating the user!");

            }
            return BadRequest();


        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto user, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please provide the user email and password correctly!");
            }
            var usr = await _userManager.FindByNameAsync(user.UserName);
            if (usr is null)
            {
                return Unauthorized();
            }
            var result = await _signInManager.CheckPasswordSignInAsync(usr, user.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                if (result.IsNotAllowed)
                {
                    throw new UnauthorizedAccessException("Please confirm your email first!");
                }
                return Unauthorized();
            }

            var userEntity = await _userManager.FindByNameAsync(user.UserName);

            if (userEntity is null)
            {
                return Unauthorized();
            }
            var accessToken = await _tokenService.CreateToken(userEntity);
            var refreshToken = await _tokenService.GenerateRefreshToken(userEntity, cancellationToken);

            return Ok(new TokenDto() { AccessToken = accessToken, RefreshToken = refreshToken });
        }


        [HttpPost("Refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] TokenDto model, CancellationToken cancellationToken)
        {
            var claims = _tokenService.GetPrincipalFromExpiredToken(model.AccessToken);
            var username = claims.FirstOrDefault(s => s.Type == ClaimTypes.Name).Value;
            if (username is null)
            {
                throw new Exception("Username not found in claims!");
            }


            var user = await _userManager.FindByNameAsync(username);
            if (user is null)
            {
                return Unauthorized();
            }
            //invalidate current refresh Token
            await _tokenService.FindAndInValidateRefreshToken(model.RefreshToken,cancellationToken);


            var newAccessToken = await _tokenService.ReGenerateAccessTokenFromExisting(claims, user);
            var newRefreshToken = await _tokenService.GenerateRefreshToken(user, cancellationToken);


            return Ok(new TokenDto() { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] TokenDto model, CancellationToken cancellationToken)
        {

            var claims = _tokenService.GetPrincipalFromExpiredToken(model.AccessToken);
            var username = claims.FirstOrDefault(s => s.Type == ClaimTypes.Name).Value;
            if (username is null)
            {
                throw new Exception("Username not found in claims!");
            }


            var user = await _userManager.FindByNameAsync(username);
            if (user is null)
            {
                return Unauthorized();
            }
            //invalidate current refresh Token
            await _tokenService.FindAndInValidateRefreshToken(model.RefreshToken, cancellationToken);


            return Ok("LogOut Successfully");
        }

        private async Task SendConfirmationEmail(string? email, ApplicationUser? user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var httpContext = _httpContextAccessor.HttpContext;
            var url = _urlHelperFactory.GetUrlHelper(new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor()));
            var confirmationLink = url.Action(
                "ConfirmEmail",
                "Authentication",
                new { UserId = user.Id, Token = token },
                protocol: HttpContext.Request.Scheme
            );

            await _emailService.SendEmailAsync(
                email,
                "Confirm Your Email",
                $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.",
                true
            );


        }
        [AllowAnonymous]
        [HttpGet("confirm-email")]
        public async Task<String> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (userId == null || token == null)
            {
                return "Link expired";
            }
            else if (user == null)
            {
                return "User not Found";
            }
            else
            {
                token = token.Replace(" ", "+");
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return "Thank you for confirming your email";
                }
                else
                {
                    return "Email not confirmed";
                }
            }
        }

        [AllowAnonymous]
        [HttpGet("send-forgotPassword-email")]
        public async Task SendForgotPasswordEmail(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    throw new InvalidOperationException("The user does not exists! Please, register first!");
                }
                var httpContext = _httpContextAccessor.HttpContext;
                var url = _urlHelperFactory.GetUrlHelper(new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor()));
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var passwordResetLink = url.Action(
               "ResetPassword",
               "Authentication",
               new { Email = user.Email, Token = token },
               protocol: HttpContext.Request.Scheme
           );
                await _emailService.SendEmailAsync(email, "Reset Your Password", $"Reset your password by <a href='{passwordResetLink}'>clicking here</a>.", true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AllowAnonymous]
        [HttpGet("ResetPassword")]
        public async Task<bool> ResetPassword([FromQuery] string Email, string Token)
        {
            try
            {
                //model should come from frontend
                ResetPasswordDto model = new ResetPasswordDto()
                {
                    Email = Email,
                    Token = Token,
                    Password = "TestingPurposes123!",
                    ConfirmPassword = "TestingPurposes123!"
                };
                var user = await _userManager.FindByEmailAsync(model.Email);
                model.Token = model.Token.Replace(" ", "+");
                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
