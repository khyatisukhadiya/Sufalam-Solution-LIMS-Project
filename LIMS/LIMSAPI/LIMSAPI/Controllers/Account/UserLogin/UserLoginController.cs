using System.Net;
using LIMSAPI.Models.Account.UserLogin;
using LIMSAPI.ServiceLayer.Account.UserLogin;
using Microsoft.AspNetCore.Mvc;

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

            try
            {
                _userLoginSL.UserLogin(loginModal);
                return Ok(new { message = "successfully login" });
            }
            catch(Exception ex)
            {
                return Error(ex.Message, HttpStatusCode.InternalServerError);
            }
          
        }


        [HttpPost]
        public IActionResult ChangeUserPassword([FromForm] string newPassword, [FromForm]string toEmail)
        {

            //if (!ModelState.IsValid)
            //{
            //    var validationErrors = ModelState.Values
            //        .SelectMany(v => v.Errors)
            //        .Select(e => e.ErrorMessage)
            //        .ToList();

            //    return BadRequest(new { success = false, errors = validationErrors });
            //}

            //if (string.IsNullOrEmpty(toEmail))
            //{
            //    throw new ArgumentException("ToEMail is required");
            //}

            //if (string.IsNullOrEmpty(newPassword))
            //{
            //    throw new ArgumentException("newPassWord is required");
            //}

            try
            {
                _userLoginSL.ChangeUserPassword(toEmail, newPassword);
                return Ok(new { message = "Password Update successfully" });
            }
            catch(Exception ex)
            {
                return Error(ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}
