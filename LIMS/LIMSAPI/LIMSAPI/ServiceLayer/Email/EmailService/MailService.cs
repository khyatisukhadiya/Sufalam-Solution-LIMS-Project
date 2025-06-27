using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using LIMSAPI.Helpers.Email;
using LIMSAPI.RepositryLayer.Email.EmailRepositry;

namespace LIMSAPI.ServiceLayer.Email.EmailService
{
    public class MailService : IMailService
    {
        public readonly IMailRepositry _emailRepositry;

        public MailService(IMailRepositry emailRepositry)
        {
            _emailRepositry = emailRepositry;
        }


        public string GenerateOtp()
        {
            return _emailRepositry.GenerateOtp();
        }


        public Task SendEmail(MailRequest mailRequest)
        {
            return _emailRepositry.SendEmail(mailRequest);
        }


        public Task SendEmailOtp(string toEmail, string otp) 
        {
            return _emailRepositry.SendEmailOtp(toEmail, otp);
        }


        public bool VerifyOtp(string enteredOtp, string storedOtp, DateTime timestamp)
        {
            return _emailRepositry.VerifyOtp(enteredOtp, storedOtp, timestamp);
        }
    }
}
