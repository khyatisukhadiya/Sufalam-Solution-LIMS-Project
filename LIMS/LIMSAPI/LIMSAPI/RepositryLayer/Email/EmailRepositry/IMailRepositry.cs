using LIMSAPI.Helpers.Email;

namespace LIMSAPI.RepositryLayer.Email.EmailRepositry
{
    public interface IMailRepositry
    {
        Task SendEmail(MailRequest mailRequest);
    }
}
