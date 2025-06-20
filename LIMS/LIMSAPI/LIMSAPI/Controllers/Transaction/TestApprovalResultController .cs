using System.Net;
using LIMSAPI.ServiceLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LIMSAPI.Controllers.Transaction
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestApprovalResultController : BaseAPIController
    {
        private readonly LIMSServiceInterface _limsSL;

        public TestApprovalResultController(LIMSServiceInterface limsSL, IConfiguration configuration) : base(configuration)
        {
            _limsSL = limsSL;
        }

        [HttpGet]
        public IActionResult GetApprovalResultBySampleRegisterId(int sampleRegisterId)
        {
            if (sampleRegisterId <= 0)
            {
                return Error("Invalid TestresultId provided.", HttpStatusCode.BadRequest);
            }

            try
            {
                var testresult = _limsSL.GetApprovalResultBySampleRegisterId(sampleRegisterId);

                if (testresult == null)
                {
                    return Error("TestApprovalResult not found.", HttpStatusCode.NotFound);
                }

                return Success("TestApprovalResult fetched successfully.", testresult);
            }
            catch (Exception ex)
            {
                return Error("An unexpected error occurred: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }


    
    }
}