using LIMSAPI.Models.Account.UserLogin;
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

                string query = "SELECT COUNT(*) FROM userRegistration WHERE Email = @Email AND Password = @Password";

                using (SqlCommand command = new SqlCommand(query, _sqlConnection))
                {
                    command.Parameters.AddWithValue("@Email", userLoginModal.Email);
                    command.Parameters.AddWithValue("@Password", userLoginModal.Password);

                    int count = (int)command.ExecuteScalar();

                    if (count > 0)
                    {
                       
                        response.Email = userLoginModal.Email;
                        response.Password = userLoginModal.Password;
                        response.RememberMe = userLoginModal.RememberMe;
                        
                        return response;
                    }
                    else
                    {
                        throw new Exception("UserId & Password is not correct. Try again.");
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


    }
}
