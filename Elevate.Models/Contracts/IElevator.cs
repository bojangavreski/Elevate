using Elevate.Models.Models;

namespace Elevate.Models.Contracts
{
    public interface IElevator
    {
        int Id { get; }

        /// <summary>
        /// Returns the calculated cost for the elevator based on the request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        int CalculateCost(ElevatorRequest request);

        /// <summary>
        /// Enqueues the request in the specific elevator and starts the movement if it is idle
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task EnqueueRequest(ElevatorRequest request, CancellationToken cancellationToken);


        /// <summary>
        /// Validates the request and checks if the elevator is able to handle that request
        /// </summary>
        /// <param name="elevatorRequest"></param>
        /// <returns></returns>
        bool CanEnqueue(ElevatorRequest elevatorRequest);
    }
}
