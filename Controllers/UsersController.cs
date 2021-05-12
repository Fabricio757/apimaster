using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            var response = _userService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);

        }
        [Authorize]
        [HttpPost("Post")]
        public IActionResult Post(InputJsonRequest model)
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        //[Authorize]
        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers(InputJsonRequest model)
        {
            var users = _userService.GetAll();
            return Ok(users);
        }


        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [Authorize]
        [HttpGet("{id}", Name = "GetById")]
        public IActionResult GetById(int id)
        {
            var users = _userService.GetById(id);
            return Ok(users);
        }

    }
}
