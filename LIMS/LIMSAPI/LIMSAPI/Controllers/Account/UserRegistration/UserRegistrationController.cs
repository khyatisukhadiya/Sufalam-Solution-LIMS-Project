using System.Net;
using LIMSAPI.Models.Account.UserRegistration;
using LIMSAPI.Models.Master;
using LIMSAPI.ServiceLayer.Account.UserRegistration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Twilio.TwiML.Messaging;

namespace LIMSAPI.Controllers.Account.UserRegistration
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRegistrationController : BaseAPIController
    {
        public readonly IUserRegistrationSL _userRegistrationSL;

        public UserRegistrationController(IUserRegistrationSL userRegistrationSL, IConfiguration configuration) : base(configuration)
        {
            _userRegistrationSL = userRegistrationSL;
        }


        [HttpPost]
        public IActionResult AddUserRegistration(UserRegistrationModal userRegistrationModal)
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

            var isDuplicate = _userRegistrationSL.IsDuplicate(
                table: "userRegistration",
                nameCol: "UserName",
                codeCol: "Email",
                nameVal: userRegistrationModal.UserName,
                codeVal: userRegistrationModal.Email,
                excludeId: userRegistrationModal.UserId,
                idCol: "UserId");

            if (isDuplicate)
            {
                errors.Add("The user already exists.");
            }

            if (errors.Any())
            {
                return BadRequest(new { Success = false, errors });
            }

            try
            {
               var result = _userRegistrationSL.AddUserRegistration(userRegistrationModal);
                return Ok( new { message= "User Register Successfully..!", result});
            }
            catch (Exception ex)
            {
                return Error(ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}
