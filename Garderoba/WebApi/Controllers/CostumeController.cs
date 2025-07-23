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
                    DateCreated = DateTime.UtcNow
                };

                var costumePart = model.Parts.Select(p => new CostumePart
                {
                    Region = p.Region,
                    Name = p.Name,
                    PartNumber = p.PartNumber,
                    DateCreated = DateTime.UtcNow
                }).ToList();

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
    }
}
