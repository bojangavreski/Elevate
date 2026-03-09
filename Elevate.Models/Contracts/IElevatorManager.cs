using Elevate.Models.Models;

namespace Elevate.Serices.Contracts
{
    public interface IElevatorManager
    {
        Task RequestElevator(ElevatorRequest elevatorRequest, CancellationToken cancellationToken);

        Task StartElevatorLoop(CancellationToken cancellationToken);
    }
}
