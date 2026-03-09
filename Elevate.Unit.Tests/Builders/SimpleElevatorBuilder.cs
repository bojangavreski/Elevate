using Elevate.Models.Contracts;
using Elevate.Models.Models;
using Elevate.Serices.Services;
using Elevate.Unit.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace Elevate.Unit.Tests.Builders
{
    public class SimpleElevatorBuilder
    {
        private readonly Mock<ILogger<SimpleElevator>> _mockLogger = new();
        private readonly Mock<INotificationService> _mockNotificationService = new();

        private IDelayProvider _delayProvider = new FakeDelayProvider();
        private int _currentFloor = 1;
        private double _stopDelaySeconds = 0;
        private double _movementDelaySeconds = 0;

        public SimpleElevatorBuilder WithCurrentFloor(int currentFloor)
        {
            _currentFloor = currentFloor;
            return this;
        }

        public SimpleElevatorBuilder WithStopDelaySeconds(double stopDelaySeconds)
        {
            _stopDelaySeconds = stopDelaySeconds;
            return this;
        }

        public SimpleElevatorBuilder WithMovementDelaySeconds(double movementDelaySeconds)
        {
            _movementDelaySeconds = movementDelaySeconds;
            return this;
        }

        public SimpleElevatorBuilder WithDelayProvider(IDelayProvider delayProvider)
        {
            _delayProvider = delayProvider;
            return this;
        }

        public SimpleElevator Build()
        {
            _mockNotificationService.Setup(sp => sp.Stop(It.IsAny<int>())).Returns(Task.CompletedTask);
            _mockNotificationService.Setup(sp => sp.RequestEnqueued(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockNotificationService.Setup(sp => sp.SetIdle(It.IsAny<int>())).Returns(Task.CompletedTask);
            _mockNotificationService.Setup(sp => sp.Move(It.IsAny<int>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockNotificationService.Setup(sp => sp.RemoveRequests(It.IsAny<IEnumerable<Guid>>())).Returns(Task.CompletedTask);

            var elevator = new SimpleElevator(
                1, // id
                _mockLogger.Object,
                _delayProvider,
                _mockNotificationService.Object
            );

            SetPrivateField(elevator, "CurrentFloor", _currentFloor);
            SetPrivateField(elevator, "StopDelaySeconds", _stopDelaySeconds);
            SetPrivateField(elevator, "MovementDelaySeconds", _movementDelaySeconds);

            return elevator;
        }

        private static void SetPrivateField(SimpleElevator elevator, string fieldName, object value)
        {
            var field = typeof(SimpleElevator).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(elevator, value);
                return;
            }

            // Try to find it in the base class as a property
            var property = typeof(BaseElevator).GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null)
            {
                property.SetValue(elevator, value);
                return;
            }

            throw new InvalidOperationException($"Field or property '{fieldName}' not found");
        }
    }
}
