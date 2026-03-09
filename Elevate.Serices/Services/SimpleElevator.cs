using Elevate.Models.Contracts;
using Elevate.Models.Enums;
using Elevate.Models.Models;
using Elevate.Serices.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Elevate.Serices.Services
{
    public class SimpleElevator : BaseElevator
    {
        private double MovementDelaySeconds = 2;
        private double StopDelaySeconds = 2;

        private readonly List<ElevatorRequest> _activeRequests = new List<ElevatorRequest>();
        private readonly SemaphoreSlim _movementLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _elevatorInitializationLock = new SemaphoreSlim(1, 1);

        private readonly ILogger<SimpleElevator> _logger;
        private readonly IDelayProvider _delayProvider;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private bool _isMoving = false;

        public SimpleElevator(int id, 
                              ILogger<SimpleElevator> logger,
                              IDelayProvider delayProvider,
                              IServiceScopeFactory serviceScopeFactory) : base(id)
        {
            CurrentFloor = 1;
            Direction = ElevatorDirectionType.Idle;
            _logger = logger;
            _delayProvider = delayProvider;
            _serviceScopeFactory = serviceScopeFactory;
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
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!CanEnqueue(request))
            {
                throw new ArgumentException(
                    $"Invalid elevator request: From={request.From}, To={request.To}. " +
                    $"Floors must be between {MinFloor} and {MaxFloor} and should not be equal");
            }

            await AddRequest(request, cancellationToken);
        }

        private async Task AddRequest(ElevatorRequest request, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;

            bool lockAcquired = false;
            try
            {
                await _elevatorInitializationLock.WaitAsync(cancellationToken);
                lockAcquired = true;

                _activeRequests.Add(request);

                if(!_isMoving)
                {
                    InitializeElevator(request, cancellationToken);
                }

                using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                {
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await notificationService.RequestEnqueued(Id, request.From, request.To, request.Uid.ToString());
                }

                _logger.LogInformation($"==== Elevator {Id} ==== Request From: {request.From} To: {request.To}");
            }
            catch (OperationCanceledException)
            {
                if (lockAcquired)
                {
                    _activeRequests.Remove(request);
                }
            }
            finally
            {
                if (lockAcquired)
                {
                    _elevatorInitializationLock.Release();
                }
            }
        }

        private void InitializeElevator(ElevatorRequest request, CancellationToken cancellationToken)
        {
            _isMoving = true;

            if (Direction == ElevatorDirectionType.Idle)
            {
                Direction = request.From > CurrentFloor ? ElevatorDirectionType.Up :
                            request.From < CurrentFloor ? ElevatorDirectionType.Down :
                            request.GetDirection();
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await Move(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    //await HandleMovementCancelled();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"==== Elevator {Id} ==== Movement failed unexpectedly.");
                    //await HandleMovementFailed(ex);
                }
            }, CancellationToken.None);
        }

        private async Task Move(CancellationToken cancellationToken)
        {
            while (true)
            {
                await _movementLock.WaitAsync(cancellationToken);
                try
                {
                    if (_activeRequests.Count == 0)
                    {
                        _isMoving = false;
                        break;
                    }

                    // If the elevator was idle, determine whether should we embark passengers on the current floor before moving
                    await CurrentFloorStop(cancellationToken);

                    if (Direction == ElevatorDirectionType.Idle)
                    {
                        _isMoving = false;
                        break;
                    }

                    // Move one floor in the current direction
                    int nextFloor = Direction == ElevatorDirectionType.Up ?
                                                 CurrentFloor + 1 :
                                                 CurrentFloor - 1;

                    // Release lock during delay
                    _movementLock.Release();

                    // Wait for movement 
                    await _delayProvider.Delay(TimeSpan.FromSeconds(MovementDelaySeconds), cancellationToken);

                    await _movementLock.WaitAsync(cancellationToken);

                    CurrentFloor = nextFloor;

                    using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                    {
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        await notificationService.Move(Id, Direction.ToString().ToLower());
                    }

                    _logger.LogInformation($"Elevator {Id} moved to floor {CurrentFloor}");

                    UpdateDirection();

                    // Check if we need to stop at this floor
                    await CurrentFloorStop(cancellationToken);

                    // If no more requests, stop moving
                    if (Direction == ElevatorDirectionType.Idle)
                    {
                        _isMoving = false;

                        using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                        {
                            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                            await notificationService.SetIdle(Id);
                        }

                        break;
                    }
                }
                finally
                {
                    _movementLock.Release();
                }
            }
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

            // Wait for embark/disembark if anyone got on or off
            if (toDisembark.Count > 0 ||
                toEmbark.Count > 0)
            {
                using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                {
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await notificationService.Stop(Id); // It is really tough to handle both embark and disembark in the frontend

                    await notificationService.RemoveRequests(toDisembark.Select(x => x.Uid));
                }

                await _delayProvider.Delay(TimeSpan.FromSeconds(StopDelaySeconds), cancellationToken);
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
