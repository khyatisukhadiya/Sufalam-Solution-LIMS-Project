using System.Net;
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
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .Select(v => v.Errors[0])
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { success = false, errors = validationErrors });
            }

            try
            {
                _sMSService.sendSMS(sMSReruest);
                return Ok(new { message = "SMS Sent Successfully" });
            }
            catch (Exception ex)
            {
                return Error(ex.Message, HttpStatusCode.InternalServerError);
            }
           
        }
    }
}
