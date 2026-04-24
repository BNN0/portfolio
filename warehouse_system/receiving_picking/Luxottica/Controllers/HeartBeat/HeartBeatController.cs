using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Luxottica.Controllers.HeartBeat
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeartBeatController : ControllerBase
    {

        // GET api/<ValuesController>/5
        [HttpGet]
        public int Get()
        {
            return 1;
        }

    }
}
