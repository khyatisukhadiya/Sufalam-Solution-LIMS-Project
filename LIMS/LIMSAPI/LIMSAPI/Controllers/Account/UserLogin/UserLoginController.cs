using System.Net;
using LIMSAPI.Models.Account.UserLogin;
using LIMSAPI.ServiceLayer.Account.UserLogin;
using Microsoft.AspNetCore.Mvc;
using Twilio.TwiML.Messaging;

namespace LIMSAPI.Controllers.Account.UserLogin
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class UserLoginController : BaseAPIController
    {
        public readonly IUserLoginSL _userLoginSL;

        public UserLoginController(IUserLoginSL userLoginSL,IConfiguration configuration) : base(configuration) 
        {
            _userLoginSL = userLoginSL;
        }


        [HttpPost]
        public IActionResult UserLogin(UserLoginModal loginModal)
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
               var result = _userLoginSL.UserLogin(loginModal);
                string message = "Login Successfully.....";
                return Success(message,result);
            }
            catch(Exception ex)
            {
                return Error(ex.Message, HttpStatusCode.InternalServerError);
            }
          
        }


        [HttpPost]
        public IActionResult ChangeUserPassword([FromForm] string newPassword, [FromForm]string toEmail)
        {

            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { success = false, errors = validationErrors });
            }

            if (string.IsNullOrEmpty(toEmail))
            {
                throw new ArgumentException("ToEMail is required");
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException("newPassWord is required");
            }

            var errors = new List<string>();

            if (errors.Any())
            {
                return BadRequest(new { errors });
            }

            try
            {
               var result = _userLoginSL.ChangeUserPassword(toEmail, newPassword);
               string message = "Password Update successfully";
                return Success(message,result);
            }
            catch(Exception ex)
            {
                return Error(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        public IActionResult GetUserLogindetails([FromForm] string UserName, [FromForm]string Password)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .SelectMany(v => v.Errors)  
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { success = false, errors = validationErrors });
            }

            if (string.IsNullOrEmpty(UserName))
            {
                throw new ArgumentException("ToEMail is required");
            }

            if (string.IsNullOrEmpty(Password))
            {
                throw new ArgumentException("newPassWord is required");
            }

            try
            {
                var result = _userLoginSL.GetUserLoginDetails(UserName, Password);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return Error(ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}
