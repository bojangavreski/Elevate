using Elevate.Models.Contracts;
using Elevate.Models.Models;
using Elevate.Serices.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Elevate.Unit.Tests
{
    public class ElevatorManagerTests
    {
        [Fact]
        public async Task RequestElevator_SelectsElevatorWithLowestCost()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ElevatorManager>>();

            var mockElevator1 = CreateMockElevator(1, 10);
            var mockElevator2 = CreateMockElevator(2, 3);
            var mockElevator3 = CreateMockElevator(3, 7);

            var elevators = new List<IElevator> { mockElevator1.Object, mockElevator2.Object, mockElevator3.Object };
            var manager = new ElevatorManager(elevators, mockLogger.Object);

            var request = new ElevatorRequest { From = 5, To = 8 };

            // Act
            await manager.RequestElevator(request, CancellationToken.None);

            // Assert
            mockElevator1.Verify(e => e.EnqueueRequest(request, It.IsAny<CancellationToken>()), Times.Never);
            mockElevator2.Verify(e => e.EnqueueRequest(request, It.IsAny<CancellationToken>()), Times.Once);
            mockElevator3.Verify(e => e.EnqueueRequest(request, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RequestElevator_EnqueueMultipleRequestsInASingleElevator()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ElevatorManager>>();
            
            var mockElevator1 = CreateMockElevator(1, 2);
            var mockElevator2 = CreateMockElevator(2, 5);

            var elevators = new List<IElevator> { mockElevator1.Object, mockElevator2.Object };
            var manager = new ElevatorManager(elevators, mockLogger.Object);

            var request1 = new ElevatorRequest { From = 3, To = 7 };
            var request2 = new ElevatorRequest { From = 5, To = 9 };
            var request3 = new ElevatorRequest { From = 2, To = 6 };

            // Act
            await manager.RequestElevator(request1, CancellationToken.None);
            await manager.RequestElevator(request2, CancellationToken.None);
            await manager.RequestElevator(request3, CancellationToken.None);

            // Assert
            mockElevator1.Verify(e => e.EnqueueRequest(It.IsAny<ElevatorRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            mockElevator2.Verify(e => e.EnqueueRequest(It.IsAny<ElevatorRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            
            // Verify each specific request was enqueued to elevator 1
            mockElevator1.Verify(e => e.EnqueueRequest(request1, It.IsAny<CancellationToken>()), Times.Once);
            mockElevator1.Verify(e => e.EnqueueRequest(request2, It.IsAny<CancellationToken>()), Times.Once);
            mockElevator1.Verify(e => e.EnqueueRequest(request3, It.IsAny<CancellationToken>()), Times.Once);
        }

        private Mock<IElevator> CreateMockElevator(int id, int cost)
        {
            var mockElevator = new Mock<IElevator>();
            mockElevator.Setup(e => e.Id).Returns(id);
            mockElevator.Setup(e => e.CalculateCost(It.IsAny<ElevatorRequest>())).Returns(cost);
            mockElevator.Setup(e => e.EnqueueRequest(It.IsAny<ElevatorRequest>(), It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);

            return mockElevator;
        }
    }
}
