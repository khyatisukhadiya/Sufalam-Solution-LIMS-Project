namespace LIMSAPI.RepositryLayer.OTP.OTPRespository
{
    public interface IOTPRepository
    {
        string GenerateOtp();

        Task SaveOtp(string toEmail, string otp, DateTime expiry);

        string VerifyOTP(string toEmail, string enteredOtp);
    }
}
