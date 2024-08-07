using Moq;
using Xunit;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using BusinessLayer.IServices;
using BusinessLayer.Services;
using DataAccessLayer.Entities;
using SharedLayer.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Linq;
using System.Security.Claims;
using JourneyAPI.Controllers;
using SharedLayer;
using DataAccessLayer.Config;
using System.IdentityModel.Tokens.Jwt;

public class AuthenticationControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUrlHelperFactory> _urlHelperFactoryMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly AuthenticationController _controller;
    private readonly DefaultHttpContext _httpContext;

    public AuthenticationControllerTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),null, null, null, null,  null, null,null, null
        );

        _tokenServiceMock = new Mock<ITokenService>();
        _httpContext= new DefaultHttpContext();

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null,
            null,
            null,
            null
        );

        _emailServiceMock = new Mock<IEmailService>();
        _urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _controller = new AuthenticationController(
            _userManagerMock.Object,
            _tokenServiceMock.Object,
            _signInManagerMock.Object,
            _emailServiceMock.Object,
            _urlHelperFactoryMock.Object,
            _httpContextAccessorMock.Object
        );
    }

    

    [Fact]
    public async Task Login_ReturnsOk_WithToken_WhenLoginSucceeds()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            UserName = "test@example.com",
            Password = "Password123!"
        };

        var user = new ApplicationUser { UserName = loginDto.UserName };
        _userManagerMock.Setup(um => um.FindByNameAsync(loginDto.UserName)).ReturnsAsync(user);
        _signInManagerMock.Setup(sm => sm.CheckPasswordSignInAsync(user, loginDto.Password, false)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        _tokenServiceMock.Setup(ts => ts.CreateToken(user)).ReturnsAsync("access-token");
        _tokenServiceMock.Setup(ts => ts.GenerateRefreshToken(user, It.IsAny<CancellationToken>())).ReturnsAsync("refresh-token");

        // Act
        var result = await _controller.Login(loginDto, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var tokenDto = Assert.IsType<TokenDto>(okResult.Value);
        Assert.Equal("access-token", tokenDto.AccessToken);
        Assert.Equal("refresh-token", tokenDto.RefreshToken);
    }

   
    [Fact]
    public async Task ConfirmEmail_ReturnsSuccessMessage_WhenEmailConfirmationSucceeds()
    {
        // Arrange
        var userId = "user-id";
        var token = "token";
        var user = new ApplicationUser();
        _userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.ConfirmEmailAsync(user, token)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.ConfirmEmail(userId, token);

        // Assert
        Assert.Equal("Thank you for confirming your email", result);
    }

    

    [Fact]
    public async Task ResetPassword_ReturnsTrue_WhenPasswordResetSucceeds()
    {
        // Arrange
        var email = "test@example.com";
        var token = "token";
        var user = new ApplicationUser { Email = email };
        _userManagerMock.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.ResetPasswordAsync(user, token, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.ResetPassword(email, token);

        // Assert
        Assert.True(result);
    }
}
