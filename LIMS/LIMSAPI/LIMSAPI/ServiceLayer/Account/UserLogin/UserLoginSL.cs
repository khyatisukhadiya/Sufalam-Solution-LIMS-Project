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

        public string ChangeUserPassword(string toEmail, string newPassword)
        {
            return _userLoginRL.ChangeUserPassword(toEmail, newPassword);
        }

        public UserLoginModal UserLogin(UserLoginModal userLoginModal)
        {
            return _userLoginRL.UserLogin(userLoginModal);
        }
    }
}
