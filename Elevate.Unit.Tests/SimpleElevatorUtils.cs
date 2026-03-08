using Elevate.Models.Enums;
using Elevate.Serices.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Elevate.Unit.Tests
{
    public static class SimpleElevatorUtils
    {
        public static Mock<ILogger<SimpleElevator>> _mockLogger = new Mock<ILogger<SimpleElevator>>();
        

        public static int GetCurrentFloor(SimpleElevator elevator)
        {
            return (int)typeof(SimpleElevator).BaseType!
                .GetProperty("CurrentFloor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(elevator)!;
        }

        public static ElevatorDirectionType GetDirection(SimpleElevator elevator)
        {
            return (ElevatorDirectionType)typeof(SimpleElevator).BaseType!
                .GetProperty("Direction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(elevator)!;
        }

        public static double GetMovementDelaySeconds(SimpleElevator elevator)
        {
            return (double)typeof(SimpleElevator)!
                .GetField("MovementDelaySeconds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(elevator)!;
        }

        public static double GetStopDelaySeconds(SimpleElevator elevator)
        {
            return (double)typeof(SimpleElevator)!
                .GetField("StopDelaySeconds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(elevator)!;
        }

    }
}
