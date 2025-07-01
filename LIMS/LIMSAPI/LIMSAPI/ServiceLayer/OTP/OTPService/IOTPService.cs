namespace LIMSAPI.ServiceLayer.OTP.OTPService
{
    public interface IOTPService
    {
        string GenerateOtp();

        Task SaveOtp(string toEmail, string otp, DateTime expiry);

        string VerifyOTP(string toEmail, string enteredOtp);
    }
}
