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

        [Authorize]
        [HttpPost("Input")]
        public IActionResult Input(InputJsonRequest model)
        {
            string dbResponse = _dbService.Input(model.Input);            
            return Ok(dbResponse);
        }

    }
}
