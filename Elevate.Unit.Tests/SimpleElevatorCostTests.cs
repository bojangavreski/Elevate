using Elevate.Models.Models;
using Elevate.Unit.Tests.Builders;

namespace Elevate.Unit.Tests
{
    public class SimpleElevatorCostTests
    {

        [Theory]
        [InlineData(1, 5, 4)]    // Elevator at floor 1, request from floor 5 = cost 4
        [InlineData(5, 1, 4)]    // Elevator at floor 5, request from floor 1 = cost 4
        [InlineData(3, 3, 0)]    // Elevator at floor 3, request from floor 3 = cost 0
        [InlineData(1, 10, 9)]   // Elevator at floor 1, request from floor 10 = cost 9
        [InlineData(10, 1, 9)]   // Elevator at floor 10, request from floor 1 = cost 9
        public void CalculateCost_WhenElevatorIsIdle_ReturnsAbsoluteDistance(
            int currentFloor, int requestFrom, int expectedCost)
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().WithCurrentFloor(currentFloor)
                                                      .Build();

            var request = new ElevatorRequest { From = requestFrom, To = requestFrom + 2 };

            // Act
            var cost = elevator.CalculateCost(request);

            // Assert
            Assert.Equal(expectedCost, cost);
        }

        [Fact]
        public async Task CalculateCost_WhenMovingUpAndRequestIsAhead_ReturnsDirectDistance()
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().WithCurrentFloor(3)
                                                      .Build();

            var existingRequest = new ElevatorRequest { From = 5, To = 8 };
            await elevator.EnqueueRequest(existingRequest, CancellationToken.None);
            
            var newRequest = new ElevatorRequest { From = 6, To = 9 };

            // Act
            var cost = elevator.CalculateCost(newRequest);

            // Assert
            // 6(From) - 3(Current) = 3 
            Assert.Equal(3, cost);
        }

        [Fact]
        public async Task CalculateCost_WhenMovingDownAndRequestIsAhead_ReturnsDirectDistance()
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().WithCurrentFloor(8)
                                                      .Build();

            var existingRequest = new ElevatorRequest { From = 5, To = 2 };
            await elevator.EnqueueRequest(existingRequest, CancellationToken.None);
            
            var newRequest = new ElevatorRequest { From = 6, To = 3 };

            // Act
            var cost = elevator.CalculateCost(newRequest);

            // Assert
            // 8(From) - 6(Current) = 2 
            Assert.Equal(2, cost);
        }

        [Fact]
        public async Task CalculateCost_WhenMovingUpButRequestIsBehind_ReturnsFullSweepCost()
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().WithCurrentFloor(3)
                                                      .Build();

            var existingRequest = new ElevatorRequest { From = 5, To = 8 };
            await elevator.EnqueueRequest(existingRequest, CancellationToken.None);
            
            var newRequest = new ElevatorRequest { From = 2, To = 1 };

            // Act
            var cost = elevator.CalculateCost(newRequest);

            // Assert
            // Cost calculation  (8 - 3) + |8 - 2| = 5 + 6 = 11
            Assert.Equal(11, cost);
        }

        [Fact]
        public async Task CalculateCost_WhenMovingDownButRequestIsAbove_ReturnsFullSweepCost()
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().WithCurrentFloor(7)
                                                      .Build();

            var existingRequest = new ElevatorRequest { From = 5, To = 2 };
            var cts = new CancellationTokenSource();

            await elevator.EnqueueRequest(existingRequest, cts.Token);

            cts.Cancel();

            // Give background task a moment to start
            await Task.Delay(10);
            
            var newRequest = new ElevatorRequest { From = 9, To = 10 };

            // Act
            var cost = elevator.CalculateCost(newRequest);

            // Assert
            // Cost calculation (7 - 2) + |2 - 9| = 5 + 7 = 12
            Assert.Equal(12, cost);
        }

        [Fact]
        public async Task CalculateCost_WithMultipleActiveRequests_ConsidersHighestDestination()
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().WithCurrentFloor(2)
                                                      .Build();

            var request1 = new ElevatorRequest { From = 3, To = 5 };
            var request2 = new ElevatorRequest { From = 4, To = 9 };
            await elevator.EnqueueRequest(request1, CancellationToken.None);
            await elevator.EnqueueRequest(request2, CancellationToken.None);
            
            var newRequest = new ElevatorRequest { From = 1, To = 0 };

            // Act
            var cost = elevator.CalculateCost(newRequest);

            // Assert
            // Cost calculation (9 - 2) + |9 - 1| = 7 + 8 = 15
            Assert.Equal(15, cost);
        }
    }
}
