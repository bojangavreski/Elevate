using Elevate.Models.Enums;
using Elevate.Models.Models;
using Elevate.Unit.Tests.Builders;
namespace Elevate.Unit.Tests
{
    public class SimpleElevatorMovementTests
    {
        [Fact]
        public async Task Movement_ElevatorMovesUpAndStopsAtRequestedFloors()
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().WithCurrentFloor(1).Build();

            // Request from floor 3 to floor 6
            var request = new ElevatorRequest { From = 3, To = 6 };

            // Act
            await elevator.EnqueueRequest(request, CancellationToken.None);

            // Give the background task time to complete all movements
            await Task.Delay(200);

            var finalFloor = SimpleElevatorUtils.GetCurrentFloor(elevator);
            var finalDirection = SimpleElevatorUtils.GetDirection(elevator);

            // Assert
            Assert.Equal(6, finalFloor);
            Assert.Equal(ElevatorDirectionType.Idle, finalDirection);
        }

        [Fact]
        public async Task Movement_ElevatorMovesDownAndStopsAtRequestedFloors()
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().WithCurrentFloor(8).Build();

            // Request from floor 7 to floor 3
            var request = new ElevatorRequest { From = 7, To = 3 };

            // Act
            await elevator.EnqueueRequest(request, CancellationToken.None);

            await Task.Delay(200);

            var finalFloor = SimpleElevatorUtils.GetCurrentFloor(elevator);
            var finalDirection = SimpleElevatorUtils.GetDirection(elevator);

            // Assert
            Assert.Equal(3, finalFloor);
            Assert.Equal(ElevatorDirectionType.Idle, finalDirection);
        }

        [Fact]
        public async Task Movement_ElevatorHandlesMultipleRequestsInSameDirection()
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().WithCurrentFloor(1).Build();

            var request1 = new ElevatorRequest { From = 3, To = 5 };
            var request2 = new ElevatorRequest { From = 4, To = 7 };
            var request3 = new ElevatorRequest { From = 6, To = 9 };

            // Act
            await elevator.EnqueueRequest(request1, CancellationToken.None);
            await elevator.EnqueueRequest(request2, CancellationToken.None);
            await elevator.EnqueueRequest(request3, CancellationToken.None);

            await Task.Delay(200);

            var finalFloor = SimpleElevatorUtils.GetCurrentFloor(elevator);
            var finalDirection = SimpleElevatorUtils.GetDirection(elevator);

            // Assert
            Assert.Equal(9, finalFloor);
            Assert.Equal(ElevatorDirectionType.Idle, finalDirection);
        }

        [Fact]
        public async Task Movement_ElevatorChangesDirectionAfterReachingTopFloor()
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().WithCurrentFloor(5).Build();

            var request1 = new ElevatorRequest { From = 8, To = 10 };
            var request2 = new ElevatorRequest { From = 9, To = 3 };

            // Act
            await elevator.EnqueueRequest(request1, CancellationToken.None);
            await elevator.EnqueueRequest(request2, CancellationToken.None);

            await Task.Delay(200);

            var finalFloor = SimpleElevatorUtils.GetCurrentFloor(elevator);
            var finalDirection = SimpleElevatorUtils.GetDirection(elevator);

            // Assert
            Assert.Equal(3, finalFloor);
            Assert.Equal(ElevatorDirectionType.Idle, finalDirection);
        }

        [Fact]
        public async Task Movement_ElevatorChangesDirectionAfterReachingBottomFloor()
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().WithCurrentFloor(8)
                                                      .Build();

            var request1 = new ElevatorRequest { From = 5, To = 1 };
            var request2 = new ElevatorRequest { From = 2, To = 7 };

            // Act
            await elevator.EnqueueRequest(request1, CancellationToken.None);
            await elevator.EnqueueRequest(request2, CancellationToken.None);

            await Task.Delay(200);

            var finalFloor = SimpleElevatorUtils.GetCurrentFloor(elevator);
            var finalDirection = SimpleElevatorUtils.GetDirection(elevator);

            // Assert
            Assert.Equal(7, finalFloor);
            Assert.Equal(ElevatorDirectionType.Idle, finalDirection);
        }

        [Fact]
        public async Task Movement_ElevatorPicksUpPassengerOnCurrentFloor()
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().WithCurrentFloor(5).Build();

            var request = new ElevatorRequest { From = 5, To = 8 };

            // Act
            await elevator.EnqueueRequest(request, CancellationToken.None);

            await Task.Delay(200);

            var finalFloor = SimpleElevatorUtils.GetCurrentFloor(elevator);
            var finalDirection = SimpleElevatorUtils.GetDirection(elevator);

            // Assert
            Assert.Equal(8, finalFloor);
            Assert.Equal(ElevatorDirectionType.Idle, finalDirection);
        }
    }
}
