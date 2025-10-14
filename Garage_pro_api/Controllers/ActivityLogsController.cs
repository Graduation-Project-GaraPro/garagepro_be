using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.LogServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityLogsController : ControllerBase
    {
        private readonly ILogService _logService;

        public ActivityLogsController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _logService.GetUserActivityLogsAsync();
            return Ok(logs);
        }
    }
}
