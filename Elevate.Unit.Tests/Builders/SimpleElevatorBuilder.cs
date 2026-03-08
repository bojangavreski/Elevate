using Elevate.Serices.Services;
using Elevate.Unit.Tests.Fakes;
using Microsoft.Extensions.Logging;
using Moq;

namespace Elevate.Unit.Tests.Builders
{
    public class SimpleElevatorBuilder
    {
        private static readonly Mock<ILogger<SimpleElevator>> _mockLogger = new Mock<ILogger<SimpleElevator>>();
        private static readonly FakeDelayProvider _fakeDelayProvider = new FakeDelayProvider();

        private readonly SimpleElevator _elevator;

        public SimpleElevatorBuilder()
        {
            _elevator = new SimpleElevator(1, _mockLogger.Object, _fakeDelayProvider);
        }

        public SimpleElevatorBuilder WithCurrentFloor(int currentFloor)
        {
            typeof(SimpleElevator).BaseType!
                .GetProperty("CurrentFloor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(_elevator, currentFloor);

            return this;
        }

        public SimpleElevatorBuilder WithStopDelaySeconds(double stopDelaySeconds)
        {
            typeof(SimpleElevator)!
                .GetField("StopDelaySeconds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(_elevator, stopDelaySeconds);

            return this;
        }

        public SimpleElevatorBuilder WithMovementDelaySeconds(double movementDelaySeconds)
        {
            typeof(SimpleElevator)!
                .GetField("MovementDelaySeconds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(_elevator, movementDelaySeconds);

            return this;
        }

        public SimpleElevator Build()
        {
            return _elevator;
        }
    }
}
