using LIMSAPI.Models.TransactionModal;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LIMSAPI.Models.Transaction
{
    public class TestResultModal
    {
        public int TestResultId { get; set; }

        public int SampleRegisterId { get; set; }

        public int ServiceId { get; set; }

        public string? ServiceName { get; set; }

        public int TestId { get; set; }

        public string? TestName { get; set; }

        public string ResultValue { get; set; }

        public string ValidationStatus { get; set; }

        public string? ValidateBy { get; set; }

        public string ? CreatedBy { get; set; }

        public bool IsActive { get; set; }        

    }

  


}
