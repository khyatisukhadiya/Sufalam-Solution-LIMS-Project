using System.Net;
using Azure.Core;
using LIMSAPI.Helpers.Email;
using LIMSAPI.Models.FinanceModal;
using LIMSAPI.RepositryLayer.OTP.OTPRespository;
using LIMSAPI.ServiceLayer.Email.EmailService;
using LIMSAPI.ServiceLayer.OTP.OTPService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Twilio.TwiML.Messaging;

namespace LIMSAPI.Controllers.Email
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class EmailController : BaseAPIController
    {

        private readonly IMailService _mailService;
        public readonly IHttpContextAccessor _httpContextAccessor;
        public readonly IOTPService _oTPService;
        public readonly IOTPRepository _oTPRepository;
        //private readonly TimeSpan _optTimeOut = TimeSpan.FromMinutes(5);

        public EmailController(IMailService mailService, IHttpContextAccessor httpContextAccessor, IOTPService oTPService,IOTPRepository oTPRepository ,IConfiguration configuration): base(configuration)
        {
            _mailService = mailService;
            _httpContextAccessor = httpContextAccessor;
            _oTPService = oTPService;
            _oTPRepository = oTPRepository;
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

            var errors = new List<string>();

            if (errors.Any())
            {
                return BadRequest(new { errors });
            }

            try
            {
               var result = _mailService.SendEmail(mailRequest);
                string message = "Email Sent Successfully";
                return Success(message,result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message, HttpStatusCode.InternalServerError);
            }
        }


        [HttpPost] 
        public IActionResult SendOtpToEmail([FromForm] string toEmail)
        {

            bool emailExists = _mailService.EmailExists(toEmail);
            if (!emailExists)
            {
                return BadRequest(new { message = "Email does not exist. Please enter correct email." });

            }


            string otp = _oTPService.GenerateOtp();
            DateTime expiry = DateTime.Now.AddMinutes(3);

            _oTPService.SaveOtp(toEmail, otp, expiry);

            
            //var httpContext = _httpContextAccessor.HttpContext;
            //if (httpContext?.Session == null)
            //{
            //    return StatusCode(500, "Session is not available.");
            //}


            //_httpContextAccessor.HttpContext?.Session.SetString("OTP", otp);

            //_httpContextAccessor.HttpContext.Session.SetString("OtpTimeOut", DateTime.Now.ToString());

            try
            {
              
              var result = _mailService.SendEmailOtp(toEmail, otp);
              string message = "OTP Sent Successfully;";
              return Success(message,result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to send OTP email.");
            }
        }


        [HttpPost]
        public IActionResult VerifyOtp([FromForm] string toEmail, [FromForm]string enteredOtp)
        {
            try
            {
                string storedOtp = _oTPRepository.VerifyOTP(toEmail, enteredOtp, DateTime.Now);

                if (enteredOtp == storedOtp)
                {
                    return Ok(new { message = "OTP verified successfully!" });
                }
                else
                {
                    return Error(message : "OTP is expired. Please create new otp.", HttpStatusCode.InternalServerError);
                }
            }
            catch(Exception)
            {
                throw new Exception("OTP not found.");
            }
        }


    }
}
