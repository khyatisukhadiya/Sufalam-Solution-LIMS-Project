using LIMSAPI.Models.Account.UserRegistration;

namespace LIMSAPI.ServiceLayer.Account.UserRegistration
{
    public interface IUserRegistrationSL
    {

        UserRegistrationModal AddUserRegistration (UserRegistrationModal userRegistrationModal);

        public bool IsDuplicate(string table, string nameCol, string codeCol, string nameVal, string codeVal, int? excludeId = null, string idCol = "Id", Dictionary<string, object> additionalConditions = null);



    }
}
