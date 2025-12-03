using Microsoft.AspNetCore.Mvc;

namespace GestiParc.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PingController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Pong");
        }
    }
}
