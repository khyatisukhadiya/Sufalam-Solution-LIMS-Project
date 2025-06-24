using LIMSAPI.Helpers.Email;
using System.Net.Mail;
using System.Net;
using LIMSAPI.Helpers.SMS;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Twilio;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;

namespace LIMSAPI.RepositryLayer.SMSRepository
{
    public class SMSRepository : ISMSRepository
    {

        public readonly SMSSettings _sMSSettings;
        public SMSRepository(IOptions<SMSSettings> sMSSettings)
        {
            _sMSSettings = sMSSettings.Value;
        }


            public Task sendSMS(SMSReruest sMSReruest)
            {
                if (string.IsNullOrWhiteSpace(_sMSSettings.FromNumber))
                {
                    throw new InvalidOperationException("Sender phonenumber address (_sMSSettings.FromNumber) is not configured.");
                }

                if (string.IsNullOrWhiteSpace(sMSReruest.toPhoneNumber))
                {
                    throw new ArgumentException("Client phonenumber is required.");
                }

                string accountSid = _sMSSettings.AccountSID;
                string authToken = _sMSSettings.AuthToken;

                TwilioClient.Init(accountSid, authToken);

                var message = MessageResource.Create(
                    to: new Twilio.Types.PhoneNumber("+91"+sMSReruest.toPhoneNumber),
                    from: new Twilio.Types.PhoneNumber(_sMSSettings.FromNumber),
                    body: sMSReruest.messageBody
                );

                return Task.CompletedTask;
            }
    }
}

