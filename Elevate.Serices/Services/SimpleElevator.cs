using Elevate.Models.Enums;
using Elevate.Models.Models;
using Elevate.Serices.Utils;
using Microsoft.Extensions.Logging;

namespace Elevate.Serices.Services
{
    public class SimpleElevator : BaseElevator
    {
        private const int MaxFloor = 10;
        private const int MovementDelaySeconds = 2;
        private const int StopDelaySeconds = 2;

        private readonly List<ElevatorRequest> _activeRequests = new List<ElevatorRequest>();
        private readonly SemaphoreSlim _movementLock = new SemaphoreSlim(1, 1);
        private readonly ILogger<SimpleElevator> _logger;
        private bool _isMoving = false;

        public SimpleElevator(int id, 
                              ILogger<SimpleElevator> logger) : base(id)
        {
            CurrentFloor = 1;
            Direction = ElevatorDirectionType.Idle;
            _logger = logger;
        }

        public override int CalculateCost(ElevatorRequest newRequest)
        {
            if (Direction == ElevatorDirectionType.Idle || !_activeRequests.Any())
            {
                return Math.Abs(CurrentFloor - newRequest.From);
            }

            if (Direction == newRequest.GetDirection())
            {
                if (Direction == ElevatorDirectionType.Up && newRequest.From >= CurrentFloor)
                    return newRequest.From - CurrentFloor;

                if (Direction == ElevatorDirectionType.Down && newRequest.From <= CurrentFloor)
                    return CurrentFloor - newRequest.From;
            }

            if (Direction == ElevatorDirectionType.Up)
            {
                int highestDest = GetHighestDestination();
                return (highestDest - CurrentFloor) + Math.Abs(highestDest - newRequest.From); 
            }
            else // Idle is already handled, so it can only be Down
            {
                int lowestDest = GetLowestDestination();
                return (CurrentFloor - lowestDest) + Math.Abs(newRequest.From - lowestDest);
            }
        }

        public async override Task EnqueueRequest(ElevatorRequest request, CancellationToken cancellationToken)
        {
            await AddRequest(request, cancellationToken);
        }

        private async Task Move(CancellationToken cancellationToken)
        {
            while (_activeRequests.Count > 0)
            {
                // If the elevator was idle, determine whether should we embark passengers on the current floor before moving
                await CurrentFloorStop(cancellationToken);

                // Move one floor in the current direction
                int nextFloor = Direction == ElevatorDirectionType.Up ?
                                             CurrentFloor + 1 :
                                             CurrentFloor - 1;

                // Wait for movement 
                await Task.Delay(TimeSpan.FromSeconds(MovementDelaySeconds), cancellationToken);

                CurrentFloor = nextFloor;

                _logger.LogInformation($"Elevator {Id} moved to floor {CurrentFloor}");

                UpdateDirection();

                // Check if we need to stop at this floor
                await CurrentFloorStop(cancellationToken);

                // If no more requests, stop moving
                if (Direction == ElevatorDirectionType.Idle)
                {
                    _isMoving = false;
                    break;
                }
            }

            _isMoving = false;
        }

        private async Task CurrentFloorStop(CancellationToken cancellationToken)
        {
            bool shouldStop = ShouldStopAtFloor(CurrentFloor);
            if (shouldStop)
            {
                // Handle embark/disembark
                await HandleFloorStop(CurrentFloor, cancellationToken);
            }
        }

        private void UpdateDirection()
        {
            if (!_activeRequests.Any())
            {
                Direction = ElevatorDirectionType.Idle;
                return;
            }

            // Keep direction until we reach the furthest floor in that direction
            if (Direction == ElevatorDirectionType.Up)
            {
                int highestDest = GetHighestDestination();
                if (CurrentFloor >= highestDest || CurrentFloor == MaxFloor)
                {
                    int lowestDest = GetLowestDestination();
                    if (lowestDest < CurrentFloor)
                    {
                        Direction = ElevatorDirectionType.Down;
                    }
                    else
                    {
                        Direction = ElevatorDirectionType.Idle;
                    }
                }
            }
            else if (Direction == ElevatorDirectionType.Down)
            {
                int lowestDest = GetLowestDestination();
                if (CurrentFloor <= lowestDest || CurrentFloor == 0)
                {
                    // Check if there are any requests above
                    int highestDest = GetHighestDestination();
                    if (highestDest > CurrentFloor)
                    {
                        Direction = ElevatorDirectionType.Up;
                    }
                    else
                    {
                        Direction = ElevatorDirectionType.Idle;
                    }
                }
            }
        }

        private async Task AddRequest(ElevatorRequest request, CancellationToken cancellationToken)
        {
            await _movementLock.WaitAsync(cancellationToken);
            try
            {
                _activeRequests.Add(request);
                
                if (!_isMoving)
                {
                    _isMoving = true;
                    
                    // Determine initial direction
                    if (Direction == ElevatorDirectionType.Idle)
                    {
                        Direction = request.From > CurrentFloor ? ElevatorDirectionType.Up :
                                    request.From < CurrentFloor ? ElevatorDirectionType.Down :
                                    request.GetDirection(); // If already on the same floor, use request direction
                    }

                    // Start movement in background
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Move(cancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                            throw new Exception("Something very bad happened"); // This is for demonstration purposes only
                        }
                    }, cancellationToken);
                }
            }
            finally
            {
                _movementLock.Release();
            }
        }

        private bool ShouldStopAtFloor(int floor)
        {
            // Stop if any passenger needs to embark or disembark at this floor
            return _activeRequests.Any(r => 
                (!r.IsHandled && r.From == floor && r.GetDirection() == Direction) || 
                (r.IsHandled && r.To == floor));
        }

        private async Task HandleFloorStop(int floor, CancellationToken cancellationToken)
        {

            var toDisembark = HandleDisembarking(floor);
            var toEmbark = HandleEmbarking(floor);            

            // Wait for embark/disembark (10 seconds) if anyone got on or off
            if (toDisembark.Count > 0 ||
                toEmbark.Count > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(StopDelaySeconds), cancellationToken);
            }
        }

        private List<ElevatorRequest> HandleDisembarking(int floor)
        {
            // Disembark passengers
            var toDisembark = _activeRequests.Where(r => r.IsHandled &&
                                                         r.To == floor).ToList();
            foreach (var request in toDisembark)
            {
                _activeRequests.Remove(request);
            }


            int disembarkCount = toDisembark.Count;
            if(disembarkCount != 0)
            {
                _logger.LogInformation($"Disembarking {disembarkCount} requests on floor {floor}");
            }

            return toDisembark;
        }

        private List<ElevatorRequest> HandleEmbarking(int floor)
        {
            var toEmbark = _activeRequests.Where(r => !r.IsHandled &&
                                                      r.From == floor &&
                                                      r.GetDirection() == Direction).ToList();
            foreach (var request in toEmbark)
            {
                request.IsHandled = true;
            }

            int embarkCount = toEmbark.Count;
            if (embarkCount != 0)
            {
                _logger.LogInformation($"Embarking {embarkCount} requests on floor {floor}");
            }

            return toEmbark;
        }


        private int GetHighestDestination()
        {
            if (!_activeRequests.Any())
            {
                return CurrentFloor;
            }
               
            return _activeRequests.Max(r => Math.Max(r.IsHandled ? CurrentFloor : r.From, r.To));
        }

        private int GetLowestDestination()
        {
            if (!_activeRequests.Any())
            {
                return CurrentFloor;
            }

            return _activeRequests.Min(r => Math.Min(r.IsHandled ? CurrentFloor : r.From, r.To));
        }

    }
}
