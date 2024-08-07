using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.IServices;
using JourneyAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SharedLayer;
using SharedLayer.Dtos;
using Xunit;

namespace Tests.Controllers
{
    
    public class JourneyControllerTests
    {
        private readonly Mock<IJourneyService> _journeyServiceMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly JourneyController _controller;

        public JourneyControllerTests()
        {
            _journeyServiceMock = new Mock<IJourneyService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _controller = new JourneyController(_journeyServiceMock.Object, _notificationServiceMock.Object);
        }


        [Fact]
        public async Task GetJourney_ReturnsOkResult_WithJourneyDto()
        {
            // Arrange
            var journeyId = new Guid("11DDAD4F-7F99-46D4-741D-08DCB3D53B3A");
            var journeyDto = new ViewJourneyDto { Id = journeyId };
            _journeyServiceMock.Setup(service => service.GetById(journeyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(journeyDto);

            // Act
            var result = await _controller.GetJourney(journeyId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ViewJourneyDto>(okResult.Value);
            Assert.Equal(journeyId, returnValue.Id);
        }


        [Fact]
        public async Task GetJourney_ReturnsNotFound_WhenJourneyDoesNotExist()
        {
            // Arrange
            var journeyId = new Guid("00000000-0000-0000-0000-000000000000");
            _journeyServiceMock.Setup(service => service.GetById(journeyId, It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new Exception("Entity not found!"));

            // Act
            var result = await _controller.GetJourney(journeyId, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }


       
        [Fact]
        public async Task AddJourney_ReturnsOkResult_WithJourneyDto()
        {
            // Arrange
            var journeyDto = new ViewJourneyDto { 
                Id = Guid.NewGuid(), 
                StartLocation = "Shkoder",
                ArrivalLocation = "Tirane",
                StartTime= DateTime.Now,
                ArrivalTime= DateTime.Now.AddHours(2),
                RouteDistance=30,
                TransportationType = SharedLayer.TransportationType.Bus,

            };

            _journeyServiceMock.Setup(service => service.Add(journeyDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(journeyDto.Id);
            _journeyServiceMock.Setup(service => service.GetById(journeyDto.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(journeyDto);

            // Act
            var result = await _controller.AddJourney(journeyDto, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ViewJourneyDto>(okResult.Value);
            //Assert.Equal(journeyDto.Id, returnValue.Id);
            Assert.Equal(journeyDto.StartLocation, returnValue.StartLocation);
            Assert.Equal(journeyDto.ArrivalLocation, returnValue.ArrivalLocation);
            Assert.Equal(journeyDto.StartTime, returnValue.StartTime);
            Assert.Equal(journeyDto.ArrivalTime, returnValue.ArrivalTime);
            Assert.Equal(journeyDto.RouteDistance, returnValue.RouteDistance);
            Assert.Equal(journeyDto.TransportationType, returnValue.TransportationType);
            Assert.Equal(Status.Enabled, returnValue.Status);
        }

        [Fact]
        public async Task DeleteJourney_ReturnsOkResult_WithSuccessMessage()
        {
            // Arrange
            var journeyId = Guid.NewGuid();
            var expectedResponse = new GenericResponseDto { Success = true };
            _journeyServiceMock.Setup(service => service.Delete(journeyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.DeleteJourney(journeyId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GenericResponseDto>(okResult.Value);
            Assert.True(response.Success);
            _notificationServiceMock.Verify(
                service => service.SendAdminNotification(ActionConstants.Deleted, journeyId, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateJourney_ReturnsOkResult_WithUpdatedJourney()
        {
            // Arrange
            var journeyId = Guid.NewGuid();
            var journeyDto = new EditJourneyDto
            {
                Id = journeyId,
                StartLocation = "Shkoder",
                ArrivalLocation = "Tirane",
                StartTime= new DateTime(2024, 12, 2),
                ArrivalTime= new DateTime(2024, 12, 2).AddHours(1.45),
                RouteDistance=30,
                TransportationType = TransportationType.Car
            };


            var updatedJourneyDto = new ViewJourneyDto
            {
                Id = journeyId,
                StartLocation = "Shkoder",
                ArrivalLocation = "Tirane",
                StartTime = new DateTime(2024, 12, 2),
                ArrivalTime = new DateTime(2024, 12, 2).AddHours(1.45),
                RouteDistance = 30,
                TransportationType = TransportationType.Car,
                Status = Status.Enabled 
            };
            _journeyServiceMock.Setup(service => service.UpdateJourney(journeyId, journeyDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(journeyId);
            _journeyServiceMock.Setup(service => service.GetById(journeyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedJourneyDto);

            // Act
            var result = await _controller.UpdateJourney(journeyId, journeyDto, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var journey = Assert.IsType<ViewJourneyDto>(okResult.Value);
            Assert.Equal(journeyId, journey.Id);
            Assert.Equal(journeyDto.StartLocation, journey.StartLocation);
            Assert.Equal(journeyDto.ArrivalLocation, journey.ArrivalLocation);
            Assert.Equal(journeyDto.StartTime, journey.StartTime);
            Assert.Equal(journeyDto.ArrivalTime, journey.ArrivalTime);
            Assert.Equal(journeyDto.RouteDistance, journey.RouteDistance);
            Assert.Equal(journeyDto.TransportationType, journey.TransportationType);
            Assert.Equal(Status.Enabled, journey.Status);
            _notificationServiceMock.Verify(
                service => service.SendNotification(ActionConstants.Updated, journeyId, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}
