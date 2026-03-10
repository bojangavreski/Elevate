using Elevate.Models.Models;

namespace Elevate.Serices.Contracts
{
    public interface IElevatorManager
    {
        /// <summary>
        /// Requests appropriate elevator to handle the reques
        /// </summary>
        /// <param name="elevatorRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RequestElevator(ElevatorRequest elevatorRequest, CancellationToken cancellationToken);


        /// <summary>
        /// Starts the elevator loop with randomly generated requests
        /// Voids SRP and in prod should be in separate service 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StartElevatorLoop(CancellationToken cancellationToken);
    }
}
