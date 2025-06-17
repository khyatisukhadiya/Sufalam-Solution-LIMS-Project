namespace LIMSAPI.Models.Transaction
{
    public class TestResultDto
    {
        public List<SampleRegisterDto> SampleRegister { get; set; }
    }

    public class SampleRegisterDto
    {
        public int SampleRegisterId { get; set; }
        public List<ServiceDto> Services { get; set; }
    }

    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public List<TestDto> Tests { get; set; }
    }

    public class TestDto
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public string ResultValue { get; set; }
        public string ValidationStatus { get; set; }
        public string CreatedBy { get; set; }
        public string ValidateBy { get; set; }
        public bool IsActive { get; set; }
    }
}
