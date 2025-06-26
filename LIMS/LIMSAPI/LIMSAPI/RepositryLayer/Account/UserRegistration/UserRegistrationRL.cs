using LIMSAPI.Helpers;
using LIMSAPI.Models.Account.UserRegistration;
using Microsoft.Data.SqlClient;
using static Azure.Core.HttpHeader;

namespace LIMSAPI.RepositryLayer.Account.UserRegistration
{
    public class UserRegistrationRL : IUserRegistrationRL
    {

        public readonly IConfiguration _Configuration;
        public readonly SqlConnection _sqlConnection;
        public readonly DuplicateChecker _duplicateChecker;

        public UserRegistrationRL(IConfiguration configuration)
        {
            _Configuration = configuration;
            _sqlConnection = new SqlConnection(_Configuration["ConnectionStrings:DefaultConnection"]);
            _duplicateChecker = new DuplicateChecker(configuration);
        }


        public bool IsDuplicate(string table, string nameCol, string codeCol, string nameVal, string codeVal, int? excludeId = null, string idCol = "Id", Dictionary<string, object> additionalConditions = null)

        {
            return _duplicateChecker.IsDuplicate(table, nameCol, codeCol, nameVal, codeVal, excludeId, idCol, additionalConditions);
        }


        public UserRegistrationModal AddUserRegistration(UserRegistrationModal userRegistrationModal)
        {
           var response = new UserRegistrationModal();

            try
            {
                if(_sqlConnection.State != System.Data.ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = @"INSERT INTO userRegistration(UserName, FullName, PhoneNumber, Email, Password, ConfirmPassword, Gender, DOB) OUTPUT INSERTED.UserId 
                 VALUES (@UserName, @FullName, @PhoneNumber, @Email, @Password, @ConfirmPassword, @Gender, @DOB)";


                SqlCommand command = new SqlCommand(query, _sqlConnection);
                command.Parameters.AddWithValue("@UserName", userRegistrationModal.UserName);
                command.Parameters.AddWithValue("@FullName", userRegistrationModal.FullName);
                command.Parameters.AddWithValue("@PhoneNumber", userRegistrationModal.PhoneNumber);
                command.Parameters.AddWithValue("@Email", userRegistrationModal.Email);
                command.Parameters.AddWithValue("@Password", userRegistrationModal.Password);
                command.Parameters.AddWithValue("@ConfirmPassword", userRegistrationModal.ConfirmPassword);
                command.Parameters.AddWithValue("@Gender", userRegistrationModal.Gender);
                command.Parameters.AddWithValue("@DOB", userRegistrationModal.DOB);


                int insertedId = (int)command.ExecuteScalar();

                response.UserId = insertedId;
                response.UserName = userRegistrationModal.UserName;
                response.FullName = userRegistrationModal.FullName;
                response.PhoneNumber = userRegistrationModal.PhoneNumber;
                response.Email = userRegistrationModal.Email;
                response.Password = userRegistrationModal.Password;
                response.ConfirmPassword = userRegistrationModal.ConfirmPassword;
                response.Gender = userRegistrationModal.Gender;
                response.DOB = userRegistrationModal.DOB;
            }
            catch (Exception ex)
            {
                throw new Exception("erroe fetching into userregistration" + ex, ex);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }
    }
}
