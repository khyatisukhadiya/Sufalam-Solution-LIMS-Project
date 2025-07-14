using LIMSAPI.RepositryLayer.OTP.OTPRespository;

namespace LIMSAPI.ServiceLayer.OTP.OTPService
{
    public class OTPService : IOTPService
    {
        public readonly IOTPRepository _oTPRepository;

        public OTPService(IOTPRepository oTPRepository)
        {
            _oTPRepository = oTPRepository;
        }

        public string GenerateOtp()
        {
            return _oTPRepository.GenerateOtp();
        }

        public Task SaveOtp(string toEmail, string otp, DateTime expiry)
        {
            return _oTPRepository.SaveOtp(toEmail, otp, expiry);
        }

        public string VerifyOTP(string toEmail, string enteredOtp, DateTime now)
        {
            return _oTPRepository.VerifyOTP(toEmail, enteredOtp, now);
        }
    }
}
