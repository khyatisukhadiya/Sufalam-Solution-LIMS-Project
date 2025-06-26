using System.Net;
using Azure.Core;
using LIMSAPI.Helpers.Email;
using LIMSAPI.Models.FinanceModal;
using LIMSAPI.ServiceLayer.Email.EmailService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LIMSAPI.Controllers.Email
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class EmailController : BaseAPIController
    {

        private readonly IMailService _mailService;

        public EmailController(IMailService mailService, IConfiguration configuration): base(configuration)
        {
            _mailService = mailService;
        }


        [HttpPost]
        public IActionResult SendEmail([FromForm] MailRequest mailRequest)
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
                _mailService.SendEmail(mailRequest);
                return Ok(new { message = "Email Sent Successfully" });
            }
            catch (Exception ex)
            {
                return Error(ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}
