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
        public async Task<ActionResult> CreateCostumeAsync([FromBody] CreateCostumeWithParts model)
        {
            try
            {
                var costume = new Costume
                {
                    Name = model.Name,
                    Area = model.Area,
                    Gender = model.Gender,
                    Status = model.Status,
                    DateCreated = DateTime.UtcNow,
                    Parts = model.Parts.Select(p => new CostumePart
                    {
                        Region = p.Region,
                        Name = p.Name,
                        PartNumber = p.PartNumber,
                        Status = p.Status,
                    }).ToList()
                };

                bool success = await _costumeService.CreateNewCostumeAsync(costume);

                if (success)
                {
                    return StatusCode(201, $"Costume part created!");
                }
                else
                {
                    return BadRequest("Costume part creation failed.");
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
        [Route("AddCostumePart/{costumeId}")]
        public async Task<ActionResult> AddCostumePartAsync(Guid costumeId, [FromBody] CostumePartCreation newPartVm)
        {
            try
            {
                var costumePart = new CostumePart
                {
                    CostumeId = costumeId,
                    Region = newPartVm.Region,
                    Name = newPartVm.Name,
                    PartNumber = newPartVm.PartNumber,
                    Status = newPartVm.Status,
                    DateCreated = DateTime.UtcNow
                };

                var result = await _costumeService.AddCostumePartAsync(costumeId, costumePart);

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
        [Route("DeleteCostume/{id}")]
        public async Task<ActionResult> DeleteCostumeAsync(Guid id)
        {
            try
            {
                var success = await _costumeService.DeleteCostumeWithPartsAsync(id);

                if (!success)
                    return NotFound(new { message = "Costume not found or could not be deleted." });

                return Ok(new { message = "Costume and its parts deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the costume.", details = ex.Message });
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
    }
}
