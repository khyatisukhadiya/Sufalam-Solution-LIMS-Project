namespace LIMSAPI.Models.Transaction
{
    public class TestApprovalResultModal
    {

        public int TestresultId { get; set; }
        public int SampleRegisterId { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public DateOnly DOB { get; set; }
        public DateOnly Date { get; set; }
        public string PhoneNumber { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public int? B2BId { get; set; }
        public string? B2BName { get; set; }

        public string? Email { get; set; }

        public List<serviceMapping> serviceMappings { get; set; } = new List<serviceMapping>();
        public List<Test> Tests { get; set; } = new List<Test>();
    }

    public class serviceMapping
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
    }

    public class Test
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public string ResultValue { get; set; }

        public string CreatedBy { get; set; }

        public string ValidateBy { get; set; }

        public bool ValidationStatus { get; set; }

        public bool IsActive { get; set; }

        public int ServiceId { get; set; }
    }

}

