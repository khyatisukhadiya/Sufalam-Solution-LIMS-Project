using LIMSAPI.Helpers.SMS;
using LIMSAPI.RepositryLayer.SMSRepository;

namespace LIMSAPI.ServiceLayer.SMS.SMSService
{
    public class SMSService : ISMSService
    {
        public readonly ISMSRepository _SMSRepository;

        public SMSService(ISMSRepository smsRepository)
        {
            _SMSRepository = smsRepository;
        }

        public Task sendSMS(SMSReruest sMSReruest)
        {
           return _SMSRepository.sendSMS(sMSReruest);
        }
    }
}
