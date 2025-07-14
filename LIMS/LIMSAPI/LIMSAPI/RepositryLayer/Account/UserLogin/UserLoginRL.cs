using LIMSAPI.Models.Account.UserLogin;
using LIMSAPI.Models.Account.UserRegistration;
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

                string query = "SELECT COUNT(*) FROM userRegistration WHERE UserName = @UserName AND Password = @Password COLLATE Latin1_General_CS_AS";

                using (SqlCommand command = new SqlCommand(query, _sqlConnection))
                {
                    //command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(userLoginModal.Email) ? (object)DBNull.Value : userLoginModal.Email);
                    command.Parameters.AddWithValue("@UserName", userLoginModal.UserName);
                    command.Parameters.AddWithValue("@Password", userLoginModal.Password);

                    int count = (int)command.ExecuteScalar();

                    if (count > 0)
                    {
                       
                        //response.Email = userLoginModal.Email;
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
                throw new Exception("Invalid Username or Password", ex);
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

                string query = "UPDATE userRegistration SET Password = @Password WHERE Email = @Email COLLATE Latin1_General_CS_AS";

                using (SqlCommand command = new SqlCommand(query, _sqlConnection))
                {
                    command.Parameters.AddWithValue("@Email", toEmail);
                    command.Parameters.AddWithValue("@Password", newPassword);
                    command.ExecuteNonQuery();
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Password change to failed", ex);
            }
            return null;
        }
        

        public UserRegistrationModal GetUserLoginDetails(string UserName, string Password)
        {
            var response = new UserRegistrationModal();
            try
            {
                if(_sqlConnection.State != System.Data.ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = "SELECT * FROM userRegistration WHERE UserName = @UserName AND Password = @Password";

                using (SqlCommand command = new SqlCommand(query, _sqlConnection))
                {

                    command.Parameters.AddWithValue("@UserName", UserName);
                    command.Parameters.AddWithValue("@Password", Password);

                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        response.UserId = Convert.ToInt32(reader["UserId"]);
                        response.UserName = reader["UserName"].ToString();
                        response.Email = reader["Email"].ToString();
                        response.DOB = DateOnly.FromDateTime(Convert.ToDateTime(reader["DOB"]));
                        response.PhoneNumber = reader["PhoneNumber"].ToString();
                        response.Gender = Convert.ToChar(reader["Gender"]);
                        response.FullName = reader["FullName"].ToString();
                    }
                    else
                    {
                        throw new Exception("Please Check the Input data . Try again.");
                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception("error accour to fetch UserregisterModal by Username", ex);
            }
            finally
            {
                _sqlConnection.Close();
            }
            return response;
        }

    }
}
