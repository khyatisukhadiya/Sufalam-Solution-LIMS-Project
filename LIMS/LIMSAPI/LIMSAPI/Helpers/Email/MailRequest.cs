namespace LIMSAPI.Helpers.Email
{
    public class MailRequest
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public IFormFile Attachments { get; set; }
    }

}
