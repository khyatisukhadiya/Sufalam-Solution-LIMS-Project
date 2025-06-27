using LIMSAPI.Models.Account.UserLogin;
using LIMSAPI.RepositryLayer.Account.UserLogin;

namespace LIMSAPI.ServiceLayer.Account.UserLogin
{
    public class UserLoginSL : IUserLoginSL
    {
        public readonly IUserLoginRL  _userLoginRL;

        public UserLoginSL(IUserLoginRL userLoginRL)
        {
            _userLoginRL = userLoginRL;
        }

        public UserLoginModal UserLogin(UserLoginModal userLoginModal)
        {
            return _userLoginRL.UserLogin(userLoginModal);
        }
    }
}
