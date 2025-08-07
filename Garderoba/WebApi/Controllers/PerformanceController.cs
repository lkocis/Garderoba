using Garderoba.Service;
using Garderoba.Service.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace Garderoba.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PerformanceController : ControllerBase
    {
        private readonly IPerformanceService _performanceService;

        public PerformanceController(IPerformanceService performanceService)
        {
            _performanceService = performanceService;
        }

        [Authorize]
        [HttpGet]
        [Route("GetMaleCostumeChoreographyCheck/{choreographyId}")]
        public async Task<IActionResult> CheckCostumePartsAvailability(Guid choreographyId)
        {
            try
            {
                var (allAvailable, missingParts) = await _performanceService.CheckIfAllNecessaryPartsInStockWithMissingListAsync(choreographyId);

                return Ok(new
                {
                    AllPartsAvailable = allAvailable,
                    MissingParts = missingParts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Greška pri provjeri dijelova kostima.", Error = ex.Message });
            }
        }
    }
}
