using LIMSAPI.Models.Account.UserLogin;
using LIMSAPI.Models.Account.UserRegistration;

namespace LIMSAPI.RepositryLayer.Account.UserLogin
{
    public interface IUserLoginRL
    {
        UserLoginModal UserLogin(UserLoginModal userLoginModal);

        string ChangeUserPassword(string toEmail, string newPassword);
        UserRegistrationModal GetUserLoginDetails(string UserName, string Password);
    }
}
