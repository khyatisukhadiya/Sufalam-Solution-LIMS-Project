using LIMSAPI.Models.Account.UserRegistration;
using LIMSAPI.RepositryLayer;
using LIMSAPI.RepositryLayer.Account.UserRegistration;

namespace LIMSAPI.ServiceLayer.Account.UserRegistration
{
    public class UserRegistrationSL : IUserRegistrationSL
    {

        public readonly IUserRegistrationRL _userRegistrationRL;

        public UserRegistrationSL(IUserRegistrationRL userRegistrationRL)
        {
            _userRegistrationRL = userRegistrationRL;
        }



        public bool IsDuplicate(string table, string nameCol, string codeCol, string nameVal, string codeVal, int? excludeId = null, string idCol = "Id", Dictionary<string, object> additionalConditions = null)
        {
            return _userRegistrationRL.IsDuplicate(table, nameCol, codeCol, nameVal, codeVal, excludeId, idCol, additionalConditions);
        }

        public UserRegistrationModal AddUserRegistration(UserRegistrationModal userRegistrationModal)
        {
            return _userRegistrationRL.AddUserRegistration(userRegistrationModal);
        }
    }
}
