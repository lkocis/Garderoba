using Garderoba.Service.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Garderoba.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ColaborativeFilteringController : ControllerBase
    {
        private readonly IColaborativeFilteringService _colaborativeFilteringService;
        private readonly IUserService _userService;
        private readonly ICostumeService _costumeService;

        public ColaborativeFilteringController(IColaborativeFilteringService colaborativeFilteringService)
        {
            _colaborativeFilteringService = colaborativeFilteringService;
        }

        [Authorize]
        [HttpGet]
        [Route("GetUserWithCostumeParts/{choreographyId}")]
        public async Task<ActionResult> FindUserWithCostumeParts(Guid choreographyId)
        {
            try
            {
                var emailsAndParts = new Dictionary<string, List<string>>();
                var recommendedUsers = await _colaborativeFilteringService.FindUserWithCostumePartsAsync(choreographyId);

                if (recommendedUsers != null)
                {
                    foreach (var userId in recommendedUsers.Keys)
                    {
                        var user = await _userService.ReadUserAsync(userId); 
                        var partNames = new List<string>();

                        foreach (var partId in recommendedUsers[userId])
                        {
                            var part = await _costumeService.GetCostumePartByIdAsync(partId); 
                            partNames.Add(part.Name);
                        }

                        emailsAndParts[user.Email] = partNames;
                    }

                    return Ok(emailsAndParts); 
                }

                return NotFound("No users found with needed costume parts.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }
        }
    }
}
