using LIMSAPI.Helpers.SMS;

namespace LIMSAPI.RepositryLayer.SMSRepository
{
    public interface ISMSRepository
    {
        Task sendSMS(SMSReruest sMSReruest);
    }
}
