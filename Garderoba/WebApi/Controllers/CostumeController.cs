using Garderoba.Model;
using Garderoba.Service.Common;
using Garderoba.WebApi.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Garderoba.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CostumeController : ControllerBase
    {
        private readonly ICostumeService _costumeService;
        private readonly IConfiguration _configuration;

        public CostumeController(ICostumeService costumeService, IConfiguration configuration)
        {
            _costumeService = costumeService;
            _configuration = configuration;
        }

        
        [Authorize]
        [HttpPost]
        [Route("CreateCostume")]
        public async Task<ActionResult> CreateNewCostumeAsync([FromBody] CreateCostume model)
        {
            try
            {
                var costume = new Costume
                {
                    Name = model.Name,
                    Area = model.Area,
                    Gender = model.Gender,
                    Status = model.Status,
                    NecessaryParts = model.NecessaryParts,
                    DateCreated = DateTime.UtcNow
                };

                bool success = await _costumeService.CreateNewCostumeAsync(costume, model.ChoreographyId);

                if (success)
                {
                    return StatusCode(201, "Costume created successfully!");
                }
                else
                {
                    return BadRequest("Costume creation failed.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost]
        [Route("UpdateCostumePart/{id}")]
        public async Task<ActionResult> UpdateCostumePartAsync(Guid id, [FromBody] UpdatedCostumePartFields updatedFields)
        {
            try
            {
                bool result = await _costumeService.UpdateCostumePartAsync(id, updatedFields);

                if (!result)
                {
                    return NotFound("Costume part not found or update failed.");
                }

                return Ok("Costume part updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost]
        [Route("AddCostumePart")]
        public async Task<ActionResult> AddCostumePartAsync([FromBody] CostumePartCreation newPartVm)
        {
            try
            {
                var costumePart = new CostumePart
                {
                    Region = newPartVm.Region,
                    Name = newPartVm.Name,
                    PartNumber = newPartVm.PartNumber,
                    Status = newPartVm.Status,
                    DateCreated = DateTime.UtcNow
                };

                var result = await _costumeService.AddCostumePartAsync(costumePart, newPartVm.CostumeId);

                if (!result)
                    return BadRequest("Failed to add costume part.");

                return Ok("Costume part added successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding costume part: {ex.Message}");
                return StatusCode(500, "Server error occurred while adding costume part.");
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("DeleteCostumePart/{id}")]
        public async Task<ActionResult> DeleteCostumePartAsync(Guid id)
        {
            try
            {
                var success = await _costumeService.DeleteCostumePartAsync(id);

                if (!success)
                {
                    return NotFound(new { message = "Costume part not found." });
                }

                return Ok(new { message = "Costume part deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the costume part.", details = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("DeleteCostumeWithParts/{costumeId}")]
        public async Task<ActionResult> DeleteCostumeWithPartsAsync(Guid costumeId)
        {
            try
            {
                var success = await _costumeService.DeleteCostumeWithPartsAsync(costumeId);

                if (success)
                    return Ok("Costume and its parts are deleted.");
                else
                    return NotFound("Costume not found or costume not deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error message: " + ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetAllCostumes/{userId}/{choreoId}")]
        public async Task<ActionResult> GetAllCostumesAsync(Guid userId, Guid choreoId)
        {
            try
            {
                var costumes = await _costumeService.GetAllCostumesAsync(userId, choreoId);

                var result = costumes.Select(c => new AllCostumes
                {
                    Id = c.Id,
                    Name = c.Name,
                    Area = c.Area,
                    Gender = c.Gender,
                    Status = c.Status
                }).ToList();

                if(result == null)
                {
                    return Ok("There are no costumes in inventory.");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetAllCostumeParts/{userId}/{costumeId}")]
        public async Task<ActionResult> GetAllCostumePartsAsync(Guid userId, Guid costumeId)
        {
            try
            {
                var costumeParts = await _costumeService.GetAllCostumePartsAsync(userId, costumeId);

                var result = costumeParts.Select(cp => new AllCostumeParts
                {
                    Id = cp.Id,
                    Region = cp.Region,
                    Name = cp.Name,
                    PartNumber = cp.PartNumber,
                    Status = cp.Status
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
