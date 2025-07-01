using LIMSAPI.Helpers.Email;

namespace LIMSAPI.ServiceLayer.Email.EmailService
{
    public interface IMailService
    {
        Task SendEmail(MailRequest mailRequest);


        Task SendEmailOtp(string toEmail, string otp);


        //string VerifyOtp(string enteredOtp);



    }

}
