using Garderoba.Model;
using Garderoba.Repository.Common;
using Garderoba.Service;
using Garderoba.Service.Common;
using Garderoba.WebApi.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

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

        [Authorize]
        [HttpDelete]
        [Route("DeleteChoreographyById/{id}")]
        public async Task<ActionResult> DeleteChoreographyByIdAsync(Guid id)
        {
            try
            {
                var success = await _choreographyService.DeleteChoreographyByIdAsync(id);
                if (success)
                {
                    return StatusCode(201, $"Choreography deleted!");
                }
                return BadRequest("Choreography deleting failed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetAllChoreographies")]
        public async Task<ActionResult> GetAllChoreographiesAsync()
        {
            try
            {
                var choreographies = await _choreographyService.GetAllChoreographiesAsync();

                var result = choreographies.Select(c => new AllChoreographies
                {
                    Name = c.Name,
                    Area = c.Area,
                    MenCostumeCount = c.MenCostumeCount,
                    WomenCostumeCount = c.WomenCostumeCount
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in fetching all choreographies" + ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize]
        [HttpPut]
        [Route("UpdateChoreographyById/{id}")]
        public async Task<ActionResult> UpdateChoreographyByIdAsync(Guid id, [FromBody] UpdatedChoreographyFields choreographyFields)
        {
            try
            {
                bool updateResult = await _choreographyService.UpdateChoreographyByIdAsync(id, choreographyFields);

                if (!updateResult)
                {
                    return NotFound("Choreography not found or update failed.");
                }

                return Ok("Choreography updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
