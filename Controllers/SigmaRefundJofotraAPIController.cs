using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SigmaJofotraAPICore.Process;

namespace SigmaJofotraAPICore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SigmaRefundJofotraAPIController
    {
        private readonly IConfiguration _configuration;
        public SigmaRefundJofotraAPIController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost(Name = "SigmaRefundJofotraAPI")]
        public Task<string> POST()
        {
            JoFotraProcess joFotraProcess = new JoFotraProcess();
            Task<string> response = joFotraProcess.processRefundJoFotra(_configuration);
            return response;
        }
    }
}
