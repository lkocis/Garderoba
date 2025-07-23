using Garderoba.Model;
using Garderoba.Service;
using Garderoba.Service.Common;
using Garderoba.WebApi.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Reflection;

namespace Garderoba.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CostumePartController : ControllerBase
    {
        private readonly ICostumePartService _costumePartService;
        private readonly IConfiguration _configuration;

        public CostumePartController(ICostumePartService costumePartService, IConfiguration configuration)
        {
            _costumePartService = costumePartService;
            _configuration = configuration;
        }
    }
}
