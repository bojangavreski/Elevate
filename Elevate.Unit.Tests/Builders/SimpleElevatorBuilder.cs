using Elevate.Models.Contracts;
using Elevate.Serices.Services;
using Elevate.Unit.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Elevate.Unit.Tests.Builders
{
    public class SimpleElevatorBuilder
    {
        private static readonly Mock<ILogger<SimpleElevator>> _mockLogger = new Mock<ILogger<SimpleElevator>>();


        private static readonly Mock<INotificationService> _mockNotificationService = new Mock<INotificationService>();

        private static readonly Mock<IServiceProvider> _mockServiceProvider = new Mock<IServiceProvider>();

        private static readonly Mock<IServiceScope> _mockServiceScope = new Mock<IServiceScope>();

        private static readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();

        private static readonly FakeDelayProvider _fakeDelayProvider = new FakeDelayProvider();

        private readonly SimpleElevator _elevator;

        public SimpleElevatorBuilder()
        {
            _mockNotificationService.Setup(sp => sp.Stop(It.IsAny<int>())).Returns(Task.CompletedTask);
            _mockNotificationService.Setup(sp => sp.RequestEnqueued(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);
            _mockNotificationService.Setup(sp => sp.SetIdle(It.IsAny<int>())).Returns(Task.CompletedTask);
            _mockNotificationService.Setup(sp => sp.Move(It.IsAny<int>(), It.IsAny<string>())).Returns(Task.CompletedTask);


            _mockServiceProvider.Setup(sp => sp.GetService(typeof(INotificationService)))
                                .Returns(_mockNotificationService.Object);

            _mockServiceScope.Setup(s => s.ServiceProvider)
                             .Returns(_mockServiceProvider.Object);

            _mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(_mockServiceScope.Object);


            _elevator = new SimpleElevator(1, _mockLogger.Object, _fakeDelayProvider, _mockServiceScopeFactory.Object);
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
