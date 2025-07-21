using Garderoba.Service.Common;
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
    }
}
