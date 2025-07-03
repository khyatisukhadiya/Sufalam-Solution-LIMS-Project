using LIMSAPI.Models.Account.UserLogin;
using LIMSAPI.Models.Account.UserRegistration;

namespace LIMSAPI.ServiceLayer.Account.UserLogin
{
    public interface IUserLoginSL
    {
        UserLoginModal UserLogin(UserLoginModal userLoginModal);

        string ChangeUserPassword(string toEmail, string newPassword);
        UserRegistrationModal GetUserLoginDetails(string UserName, string Password);
    }
}
