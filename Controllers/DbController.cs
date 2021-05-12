using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;
using WebApi.Helpers;
using Newtonsoft.Json;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DbController : ControllerBase
    {
        private IDbService _dbService;
        public DbController(IDbService dbService)
        {
            _dbService = dbService;
        }

        //[Authorize]
        [HttpPost("Input")]
        public IActionResult Input(InputJsonRequest model)
        {
            string dbResponse = _dbService.Input(model.Input);            
            return Ok(dbResponse);
        }

        [Authorize]
        [HttpGet("Get")]
        public IActionResult Get(InputJsonRequest model)
        {
            var dbResponse = _dbService.Get(model.Input);
            return Ok(dbResponse);
        }


        [Authorize]
        [HttpPost("Post")]
        public IActionResult Post(InputJsonRequest model)
        {
            string dbResponse = _dbService.Post(model.Input);
            return Ok(dbResponse);
        }

        [Authorize]
        [HttpPut("Put")]
        public IActionResult Put(InputJsonRequest model)
        {
            string dbResponse = _dbService.Put(model.Input);
            return Ok(dbResponse);
        }
    }
}
