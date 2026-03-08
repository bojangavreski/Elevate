using Elevate.Models.Contracts;
using Elevate.Models.Models;
using Elevate.Serices.Contracts;
using Microsoft.Extensions.Logging;

namespace Elevate.Serices.Services
{
    public class ElevatorManager : IElevatorManager
    {
        private readonly ILogger<ElevatorManager> _logger;
        private readonly IEnumerable<IElevator> _elevators;

        public ElevatorManager(IEnumerable<IElevator> elevators,
                               ILogger<ElevatorManager> logger)
        {
            _logger = logger;
            _elevators = elevators;
        }

        public async Task RequestElevator(ElevatorRequest elevatorRequest, CancellationToken cancellationToken)
        {
            var best = _elevators.Where(x => x.CanEnqueue(elevatorRequest))
                                 .MinBy(e => e.CalculateCost(elevatorRequest));

            if(best != null)
            {
                _logger.LogInformation($"==== Elevator {best.Id} ==== Request From: {elevatorRequest.From} To: {elevatorRequest.To}");
                await best.EnqueueRequest(elevatorRequest, cancellationToken);
            }

            _logger.LogInformation($"====Error==== Request From: {elevatorRequest.From} To: {elevatorRequest.To} cannot be handled");
        }
    }
}
