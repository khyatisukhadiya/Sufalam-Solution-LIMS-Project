using LIMSAPI.Models.Account.UserLogin;

namespace LIMSAPI.ServiceLayer.Account.UserLogin
{
    public interface IUserLoginSL
    {
        UserLoginModal UserLogin(UserLoginModal userLoginModal);
    }
}
