using LIMSAPI.Models.Account.UserLogin;

namespace LIMSAPI.RepositryLayer.Account.UserLogin
{
    public interface IUserLoginRL
    {
        UserLoginModal UserLogin(UserLoginModal userLoginModal);

        string ChangeUserPassword(string toEmail, string newPassword);
    }
}
