using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SigmaJofotraAPICore.Process;
using System.Reflection.Metadata.Ecma335;

namespace SigmaJofotraAPICore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SigmaJofotraAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public SigmaJofotraAPIController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        
        [HttpPost(Name = "SigmaJofotraAPI")]
        public Task<string> POST()
        {
            JoFotraProcess joFotraProcess = new JoFotraProcess();
            Task<string> response = joFotraProcess.processJoFotra(_configuration);
            return response;
        }
    }
}
