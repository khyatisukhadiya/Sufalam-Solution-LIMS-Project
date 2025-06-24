using LIMSAPI.Helpers.Email;
using LIMSAPI.Helpers.SMS;
using LIMSAPI.ServiceLayer.SMS.SMSService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LIMSAPI.Controllers.SMS
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class SMSController : BaseAPIController
    {
        public readonly ISMSService _sMSService;

        public SMSController(ISMSService sMSService, IConfiguration configuration) : base(configuration) 
        {
            _sMSService = sMSService;
        }



        [HttpPost]
        public IActionResult sendSMS([FromForm]SMSReruest sMSReruest)
        {
            _sMSService.sendSMS(sMSReruest);
            return Ok(new { message = "SMS Sent Successfully" });
        }
    }
}
