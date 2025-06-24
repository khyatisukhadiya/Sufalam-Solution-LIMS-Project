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

        public Task SendEmail(MailRequest mailRequest)
        {
            return _emailRepositry.SendEmail(mailRequest);
        }
    }
}
