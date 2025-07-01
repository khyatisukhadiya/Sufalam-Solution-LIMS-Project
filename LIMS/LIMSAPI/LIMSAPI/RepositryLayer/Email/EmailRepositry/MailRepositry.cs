using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;
using LIMSAPI.Helpers.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LIMSAPI.RepositryLayer.Email.EmailRepositry
{
    public class MailRepositry : IMailRepositry
    {

        public readonly MailSettings _mailSettings;
        public readonly IConfiguration _configuration;


        public MailRepositry(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public Task SendEmail(MailRequest mailRequest)
        {

            if (string.IsNullOrWhiteSpace(_mailSettings.Mail))
            {
                throw new InvalidOperationException("Sender email address (_mailSettings.Mail) is not configured.");
            }

            if (string.IsNullOrWhiteSpace(mailRequest.ToEmail))
            {
                throw new ArgumentException("Client email address is required.");
            }


            using (var message = new MailMessage())
            {

                message.From = new MailAddress(_mailSettings.Mail, _mailSettings.DisplayName);


                if (!string.IsNullOrWhiteSpace(mailRequest.ToEmail))
                {
                    message.To.Add(new MailAddress(mailRequest?.ToEmail));
                }

                message.Subject = mailRequest.Subject;
                message.Body = mailRequest.Body;
                message.IsBodyHtml = true;
               

                if (mailRequest.Attachments != null)
                {
                    var ms = new MemoryStream();
                    mailRequest.Attachments.CopyTo(ms);
                    ms.Position = 0;
                    var attachment = new Attachment(ms, mailRequest.Attachments.FileName, mailRequest.Attachments.ContentType);
                    message.Attachments.Add(attachment);
                }


                using (var smtp = new SmtpClient(_mailSettings.Host,_mailSettings.Port))
                {
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential(_mailSettings.Mail, _mailSettings.Password);
                        
                    try
                    {
                        smtp.Send(message);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
                    }
                }

                return Task.CompletedTask;
            }
        }


        public Task SendEmailOtp(string toEmail, string otp)
        {

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                throw new ArgumentException("Email address is required.");
            }

            var settings = _mailSettings;
            var smtpClient = new SmtpClient(settings.Host)
            {
                Port = settings.Port,
                Credentials = new NetworkCredential(settings.Mail, settings.Password),
                EnableSsl = true 
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(settings.Mail),
                Subject = "Your OTP Code",
                Body = $"Your OTP code is {otp}",
                IsBodyHtml = true 
            };

            mailMessage.To.Add(toEmail);
            smtpClient.Send(mailMessage);

            return Task.CompletedTask;
        }


        //public string VerifyOtp(string enteredOtp)
        //{
        //    return enteredOtp;
        //}



     

    }
}
