using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TrackingClient.Services;

namespace TrackingClient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessingController : Controller
    {
        [HttpGet("State")]
        public async Task<IActionResult> CurrentState()
        {
            Processing.State currentState = Processing.State.WaitingForUnit;
            await Task.Run(() => currentState = Global.ProcessingState);
            return Ok(currentState);
        }
    }
}
