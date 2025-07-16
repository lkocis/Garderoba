using Garderoba.Model;
using Garderoba.Service.Common;
using Garderoba.WebApi.ViewModel;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Garderoba.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Route("ReadUser/{id}")]
        public async Task<ActionResult> ReadUserAsync(Guid id)
        {
            User? user = await _userService.ReadUserAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            var foundUser = new UserById
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Area = user.Area,
                KUDName = user.KUDName,
            };

            return Ok(foundUser);
        }

        [HttpPost]
        [Route("CreateUser")]
        public async Task<ActionResult> CreateUserAsync([FromBody]RegistrationRequest request)
        {
            try
            {
                var user = new User
                {
                    Email = request.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    Area = request.Area,
                    KUDName = request.KUDName,
                    DateUpdated = DateTime.UtcNow,
                };

                bool success = await _userService.CreateUserAsync(user);
                if (success)
                {
                    return StatusCode(201, $"User created!"); 
                }
                else
                {
                    return BadRequest("User creation failed.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> LoginUserAsync([FromBody] LogingRequest request)
        {
            if(request == null)
            {
                return Unauthorized("Email and password cannot be empty!");
            }

            var user = await _userService.LoginUserAsync(request.Email, request.Password);
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            return Ok(request.Email + " logged in!"); 
        }
    }
}
