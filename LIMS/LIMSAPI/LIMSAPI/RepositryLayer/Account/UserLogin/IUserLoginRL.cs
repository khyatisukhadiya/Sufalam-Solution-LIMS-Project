using LIMSAPI.Models.Account.UserLogin;

namespace LIMSAPI.RepositryLayer.Account.UserLogin
{
    public interface IUserLoginRL
    {
        UserLoginModal UserLogin(UserLoginModal userLoginModal);
    }
}
