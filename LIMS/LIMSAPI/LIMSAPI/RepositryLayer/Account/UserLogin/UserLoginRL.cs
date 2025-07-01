using LIMSAPI.Models.Account.UserLogin;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Data.SqlClient;

namespace LIMSAPI.RepositryLayer.Account.UserLogin
{
    public class UserLoginRL : IUserLoginRL
    {
        public readonly SqlConnection _sqlConnection;
        public readonly IConfiguration _configuration;

        public UserLoginRL(IConfiguration configuration)
        {
            _configuration = configuration;
            _sqlConnection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        }

      

        public UserLoginModal UserLogin(UserLoginModal userLoginModal)
        {
            var response = new UserLoginModal();

            try
            {
                if (_sqlConnection.State != System.Data.ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = "SELECT COUNT(*) FROM userRegistration WHERE (Email = @Email OR UserName = @UserName) AND Password = @Password";

                using (SqlCommand command = new SqlCommand(query, _sqlConnection))
                {
                    command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(userLoginModal.Email) ? (object)DBNull.Value : userLoginModal.Email);
                    command.Parameters.AddWithValue("@UserName", string.IsNullOrEmpty(userLoginModal.UserName) ? (object)DBNull.Value : userLoginModal.UserName);
                    command.Parameters.AddWithValue("@Password", userLoginModal.Password);

                    int count = (int)command.ExecuteScalar();

                    if (count > 0)
                    {
                       
                        response.Email = userLoginModal.Email;
                        response.UserName = userLoginModal.UserName;
                        response.Password = userLoginModal.Password;
                        response.RememberMe = userLoginModal.RememberMe;
                        
                        return response;
                       
                    }
                    else
                    {
                        throw new Exception("Please Check the Input data . Try again.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw; 
            }
            finally
            {
               _sqlConnection.Close();
            }
        }

        public string ChangeUserPassword(string toEmail, string newPassword)
        {
            
            try
            {
                if(_sqlConnection.State != System.Data.ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = "UPDATE userRegistration SET Password = @Password WHERE Email = @Email";

                using (SqlCommand command = new SqlCommand(query, _sqlConnection))
                {
                    command.Parameters.AddWithValue("@Email", toEmail);
                    command.Parameters.AddWithValue("@Password", newPassword);
                    command.ExecuteNonQuery();
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Password change failed", ex);
            }
            return null;
        }
    }
}
