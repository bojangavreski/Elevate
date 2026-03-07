using Elevate.Models.Models;

namespace Elevate.Models.Contracts
{
    public interface IElevator
    {
        int Id { get; }

        int CalculateCost(ElevatorRequest request);

        Task EnqueueRequest(ElevatorRequest request, CancellationToken cancellationToken);
    }
}
