using Elevate.Models.Contracts;
using Elevate.Models.Models;
using Elevate.Serices.Contracts;
using System.Security.Cryptography;

namespace Elevate.Serices.Services
{
    public class ElevatorManager : IElevatorManager
    {
        private readonly IEnumerable<IElevator> _elevators;
        private bool _isLoopInitialized;

        public ElevatorManager(IEnumerable<IElevator> elevators)
        {
            _elevators = elevators;
            _isLoopInitialized = false;
        }

        public async Task StartElevatorLoop(CancellationToken cancellationToken)
        {
            if(_isLoopInitialized)
            {
                throw new InvalidOperationException($"Elevator loop already initialized");
            }

            _ = Task.Run(async () => await LoopElevators(cancellationToken), cancellationToken);
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

        private async Task LoopElevators(CancellationToken cancellationToken)
        {
            _isLoopInitialized = true;
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));

                var request = GenerateRandomRequest();

                await RequestElevator(request, cancellationToken);
            }
        }

        private ElevatorRequest GenerateRandomRequest()
        {
            ElevatorRequest request;

            do
            {
                request = new ElevatorRequest
                {
                    From = RandomNumberGenerator.GetInt32(1, 11),
                    To = RandomNumberGenerator.GetInt32(1, 11),
                };
            }
            while (request.From == request.To);

            return request;
        }
    }
}
