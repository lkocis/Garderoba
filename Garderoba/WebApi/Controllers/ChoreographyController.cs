using Garderoba.Model;
using Garderoba.Repository.Common;
using Garderoba.Service.Common;
using Garderoba.WebApi.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Garderoba.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChoreographyController : ControllerBase
    {
        private readonly IChoreographyService _choreographyService;
        private readonly IConfiguration _configuration;

        public ChoreographyController(IChoreographyService choreographyService, IConfiguration configuration)
        {
            _choreographyService = choreographyService;
            _configuration = configuration;
        }

        [Authorize]
        [HttpPost]
        [Route("CreateChoreography")]
        public async Task<ActionResult> CreateNewChoreographyAsync([FromBody] ChoreographyCreation request)
        {
            try
            {
                var choreography = new Choreography
                {
                    Name = request.Name,
                    Area = request.Area,
                    MenCostumeCount = request.MenCostumeCount,
                    WomenCostumeCount = request.WomenCostumeCount,
                    DateUpdated = DateTime.UtcNow,
                };

                bool success = await _choreographyService.CreateNewChoreographyAsync(choreography);
                if (success)
                {
                    return StatusCode(201, $"Choreography created!");
                }
                else
                {
                    return BadRequest("Choreography creation failed.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
