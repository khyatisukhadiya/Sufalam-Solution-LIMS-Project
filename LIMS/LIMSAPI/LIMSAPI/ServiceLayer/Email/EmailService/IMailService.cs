using LIMSAPI.Helpers.Email;

namespace LIMSAPI.ServiceLayer.Email.EmailService
{
    public interface IMailService
    {
        Task SendEmail(MailRequest mailRequest);


        Task SendEmailOtp(string toEmail, string otp);


        bool VerifyOtp(string enteredOtp, string storedOtp, DateTime timestamp);


        string GenerateOtp();

    }

}
