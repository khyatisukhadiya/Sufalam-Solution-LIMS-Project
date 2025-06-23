using LIMSAPI.Helpers.Email;

namespace LIMSAPI.ServiceLayer.Email.EmailRepositry
{
    public interface IMailRepositry
    {
        Task SendEmail(MailRequest mailRequest);
    }
}
