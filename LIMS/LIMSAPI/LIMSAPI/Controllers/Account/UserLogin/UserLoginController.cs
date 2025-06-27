using LIMSAPI.Models.Account.UserLogin;
using LIMSAPI.ServiceLayer.Account.UserLogin;
using Microsoft.AspNetCore.Http;
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
        public IActionResult login(UserLoginModal loginModal)
        {
            _userLoginSL.UserLogin(loginModal);
            return Ok(new { message = "successfully login" });
        }
    }
}
