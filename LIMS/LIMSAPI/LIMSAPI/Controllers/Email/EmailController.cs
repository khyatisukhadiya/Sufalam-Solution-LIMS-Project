using Azure.Core;
using LIMSAPI.Helpers.Email;
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
            _mailService.SendEmail(mailRequest);
            return Ok(new { message = "Email Sent Successfully" });
        }
    }
}
