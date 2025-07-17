using Garderoba.Model;
using Garderoba.Service.Common;
using Garderoba.WebApi.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Garderoba.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        public UserController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [Authorize]
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
        public async Task<ActionResult> CreateUserAsync([FromBody] RegistrationRequest request)
        {
            try
            {
                var user = new User
                {
                    Email = request.Email,
                    Password = request.Password,
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

        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> LoginUserAsync([FromBody] LogingRequest request)
        {
            if (request == null)
            {
                return Unauthorized("Email and password cannot be empty!");
            }

            var user = await _userService.LoginUserAsync(request.Email, request.Password);
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            // Read JWT settings from configuration
            var keyString = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(keyString))
            {
                // Optionally handle missing key gracefully
                return StatusCode(500, "JWT Key is missing in configuration.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FirstName ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                message = $"{request.Email} logged in!"
            });
        }

        [Authorize]
        [HttpPut]
        [Route("UpdateUserById/{id}")]
        public async Task<IActionResult> UpdateUserAsync(Guid id, [FromBody] UpdatedUserInfoFields request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID in URL does not match ID in request body.");
            }

            bool updateResult = await _userService.UpdateUserAsync(id, request);

            if (!updateResult)
            {
                return NotFound("User not found or update failed.");
            }

            return Ok("User updated successfully.");
        }
    }
}
