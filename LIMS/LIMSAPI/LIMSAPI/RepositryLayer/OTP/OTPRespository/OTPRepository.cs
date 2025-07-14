using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;

namespace LIMSAPI.RepositryLayer.OTP.OTPRespository
{
    public class OTPRepository : IOTPRepository
    {
        public readonly SqlConnection _sqlConnection;
        public readonly IConfiguration _Configuration;

        public OTPRepository(IConfiguration configuration)
        {
            _Configuration = configuration;
            _sqlConnection = new SqlConnection(_Configuration["ConnectionStrings:DefaultConnection"]);
        }


        public string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }


        public Task SaveOtp(string toEmail, string otp, DateTime expiry)
        {
            try
            {
                if (_sqlConnection.State != System.Data.ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }

                string query = "INSERT INTO OTPTables(Email, OTPValue, Expired) OUTPUT INSERTED.OTPId VALUES (@Email, @OTPValue, @Expired)";

                using (SqlCommand command = new SqlCommand(query, _sqlConnection))
                {
                    command.Parameters.AddWithValue("Email", toEmail);
                    command.Parameters.AddWithValue("OTPValue", otp);
                    command.Parameters.AddWithValue("Expired", expiry);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("otp not store in database",ex);
            }
            return Task.CompletedTask;
        }


        public string VerifyOTP(string toEmail, string enteredOtp, DateTime now)
        {

            if(_sqlConnection.State != System.Data.ConnectionState.Open)
            {
                _sqlConnection.Open();
            }

                string query = "SELECT TOP 1 OTPValue, Expired FROM OTPTables WHERE Email = @Email ORDER BY CreatedDate DESC";

                using (SqlCommand command = new SqlCommand(query, _sqlConnection))
                {

                    command.Parameters.AddWithValue("@Email", toEmail);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedOtp = reader["OTPValue"].ToString();
                            DateTime expiryDate = (DateTime)reader["Expired"];
 
                            //if (string.Equals(storedOtp, enteredOtp, StringComparison.OrdinalIgnoreCase))
                            //{
                            //    return storedOtp; // OTP is valid
                            //}

                             if(now < expiryDate && string.Equals(storedOtp, enteredOtp, StringComparison.OrdinalIgnoreCase))
                             {
                                  return storedOtp;
                             }
                        }
                    } 
                }
            return null; // OTP is invalid or not found
        }


    }
}
