using LIMSAPI.Helpers.Email;

namespace LIMSAPI.RepositryLayer.Email.EmailRepositry
{
    public interface IMailRepositry
    {
        Task SendEmail(MailRequest mailRequest);


        Task SendEmailOtp(string toEmail, string otp);


        bool EmailExists(string toEmail);

        //string VerifyOtp(string enteredOtp);

    }
}
