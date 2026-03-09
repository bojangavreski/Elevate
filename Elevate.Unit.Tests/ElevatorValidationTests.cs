using Elevate.Models.Models;
using Elevate.Serices.Services;
using Elevate.Unit.Tests.Builders;

namespace Elevate.Unit.Tests
{
    public class ElevatorValidationTests
    {
        [Fact]
        public async Task EnqueueRequest_WithNullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().Build();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => elevator.EnqueueRequest(null!, CancellationToken.None));
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(-1, 5)]
        [InlineData(11, 5)]
        [InlineData(100, 5)]
        public async Task EnqueueRequest_WithInvalidFromFloor_ThrowsArgumentException(int from, int to)
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().Build();
            var request = new ElevatorRequest { From = from, To = to };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => elevator.EnqueueRequest(request, CancellationToken.None));

            Assert.Contains("Invalid elevator request", exception.Message);
            Assert.Contains($"From={from}", exception.Message);
        }

        [Theory]
        [InlineData(5, 0)]
        [InlineData(5, -1)]
        [InlineData(5, 11)]
        [InlineData(5, 100)]
        public async Task EnqueueRequest_WithInvalidToFloor_ThrowsArgumentException(int from, int to)
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().Build();
            var request = new ElevatorRequest { From = from, To = to };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => elevator.EnqueueRequest(request, CancellationToken.None));

            Assert.Contains("Invalid elevator request", exception.Message);
            Assert.Contains($"To={to}", exception.Message);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(5, 5)]
        [InlineData(10, 10)]
        public async Task EnqueueRequest_WithSameFromAndToFloors_ThrowsArgumentException(int from, int to)
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().Build();
            var request = new ElevatorRequest { From = from, To = to };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => elevator.EnqueueRequest(request, CancellationToken.None));

            Assert.Contains("From must not equal To", exception.Message);
        }



        [Fact]
        public void CanEnqueue_WithNullRequest_ReturnsFalse()
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().Build();

            // Act
            var result = elevator.CanEnqueue(null!);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(5, 0)]
        [InlineData(11, 5)]
        [InlineData(5, 11)]
        [InlineData(5, 5)]
        [InlineData(-1, 5)]
        [InlineData(5, -1)]
        public void CanEnqueue_WithInvalidRequest_ReturnsFalse(int from, int to)
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().Build();
            var request = new ElevatorRequest { From = from, To = to };

            // Act
            var result = elevator.CanEnqueue(request);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(10, 1)]
        [InlineData(5, 6)]
        [InlineData(1, 2)]
        [InlineData(9, 10)]
        public void CanEnqueue_WithValidRequest_ReturnsTrue(int from, int to)
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().Build();
            var request = new ElevatorRequest { From = from, To = to };

            // Act
            var result = elevator.CanEnqueue(request);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RequestElevator_WithNullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().Build();
            var elevators = new List<Models.Contracts.IElevator> { elevator };
            var manager = new ElevatorManager(elevators);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => manager.RequestElevator(null!, CancellationToken.None));
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(2, 11)]
        [InlineData(-1, 5)]
        [InlineData(5, -1)]
        [InlineData(5, 5)]
        public async Task RequestElevator_WithInvalidRequest_ThrowsInvalidOperationException(int from, int to)
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().Build();
            var elevators = new List<Models.Contracts.IElevator> { elevator };
            var manager = new ElevatorManager(elevators);

            var request = new ElevatorRequest { From = from, To = to};

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => manager.RequestElevator(request, CancellationToken.None));

            Assert.Contains("No available elevator", exception.Message);
        }

        [Theory]
        [InlineData(1, 5)]
        [InlineData(9, 2)]
        [InlineData(3, 4)]
        public async Task RequestElevator_WithValidRequest_Succeeds(int from, int to)
        {
            // Arrange
            var elevator = new SimpleElevatorBuilder().Build();
            var elevators = new List<Models.Contracts.IElevator> { elevator };
            var manager = new ElevatorManager(elevators);

            var request = new ElevatorRequest { From = from, To = to };

            // Act & Assert - Should not throw
            await manager.RequestElevator(request, CancellationToken.None);
            
            await Task.Delay(50);
        }
    }
}
