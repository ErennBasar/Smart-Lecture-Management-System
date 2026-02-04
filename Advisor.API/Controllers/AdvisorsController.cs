using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Advisor.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdvisorsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Merhaba, ben Danışman Servisi. Pasaportun sağlam, içeri girdin.");
        }
    }
}
