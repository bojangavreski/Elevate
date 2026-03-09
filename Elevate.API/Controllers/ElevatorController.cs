using Elevate.Serices.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Elevate.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElevatorController : ControllerBase
    {
        private readonly IElevatorManager _elevatorManager;

        public ElevatorController(IElevatorManager elevatorManager)
        {
            _elevatorManager = elevatorManager;
        }

        [HttpPost("loop")]
        public async Task<ActionResult> LoopElevators(CancellationToken cancellationToken)
        {
            await _elevatorManager.StartElevatorLoop(cancellationToken);
            return Ok("Elevator simulation started successfully\nCheck output logs for simulation results");
        }
    }
}
