using AutoMapper;
using DataAccessLayer.Entities;
using JourneyAPI.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using SharedLayer.Dtos;
using SharedLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly AccountController _controller;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<IMapper> _mapperMock;

        public AccountControllerTests()
        {
            
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null
            );

            
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null,
                null,
                null,
                null
            );

          
            _mapperMock = new Mock<IMapper>();

           
            _controller = new AccountController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOkResult_WithUsers()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "58eae49f-07e7-486e-bd8a-3b08d1b61c0e", UserName = "user1@example.com", Email = "user1@example.com", Status = Status.Enabled },
                new ApplicationUser { Id = "58eae49f-07e7-486e-bd8a-3b08d1b61c0w", UserName = "user2@example.com", Email = "user2@example.com", Status = Status.Disabled }
            };
            var userDtos = users.Select(user => new ApplicationUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Status = user.Status
            }).ToList();
            _userManagerMock.Setup(um => um.Users).Returns(users.AsQueryable());
            _mapperMock.Setup(m => m.Map<ApplicationUserDto>(It.IsAny<ApplicationUser>())).Returns((ApplicationUser src) => userDtos.First(dto => dto.Id == src.Id));

            // Act
            var result = await _controller.GetAllUsers(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUserDtos = Assert.IsType<List<ApplicationUserDto>>(okResult.Value);
            Assert.Equal(1, returnedUserDtos.Count);
        }
    }
}
