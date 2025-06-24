using LIMSAPI.Helpers.SMS;

namespace LIMSAPI.ServiceLayer.SMS.SMSService
{
    public interface ISMSService
    {
        Task sendSMS(SMSReruest sMSReruest);
    }
}
