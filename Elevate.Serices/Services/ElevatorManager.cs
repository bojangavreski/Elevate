using Elevate.Models.Contracts;
using Elevate.Models.Models;
using Elevate.Serices.Contracts;
using Microsoft.Extensions.Logging;

namespace Elevate.Serices.Services
{
    public class ElevatorManager : IElevatorManager
    {
        private readonly IEnumerable<IElevator> _elevators;

        public ElevatorManager(IEnumerable<IElevator> elevators)
        {
            _elevators = elevators;
        }

        public async Task RequestElevator(ElevatorRequest elevatorRequest, CancellationToken cancellationToken)
        {
            var best = _elevators.Where(x => x.CanEnqueue(elevatorRequest))
                                 .MinBy(e => e.CalculateCost(elevatorRequest));

            if(best != null)
            {
                await best.EnqueueRequest(elevatorRequest, cancellationToken);
            }
        }
    }
}
