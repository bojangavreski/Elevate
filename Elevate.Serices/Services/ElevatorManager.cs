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
            if (elevatorRequest == null)
            {
                throw new ArgumentNullException(nameof(elevatorRequest));
            }

            var availableElevators = _elevators.Where(x => x.CanEnqueue(elevatorRequest)).ToList();

            if (!availableElevators.Any())
            {
                throw new InvalidOperationException(
                    $"No available elevator can handle request from floor {elevatorRequest.From} to {elevatorRequest.To}");
            }

            var best = availableElevators.MinBy(e => e.CalculateCost(elevatorRequest));

            if (best != null)
            {
                await best.EnqueueRequest(elevatorRequest, cancellationToken);
            }
        }
    }
}
