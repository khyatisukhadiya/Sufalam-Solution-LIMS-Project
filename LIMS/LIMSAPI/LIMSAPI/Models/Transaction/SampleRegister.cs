using System.ComponentModel.DataAnnotations;
using LIMSAPI.Models.FinanceModal;
using LIMSAPI.Models.Master;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LIMSAPI.Models.TransactionModal
{
    public class SampleRegister
    {

        public int SampleRegisterId { get; set; }

        public DateTime Date { get; set; }


        // BRANCH
        public int? BranchId { get; set; }

        public string? BranchName { get; set; }



        public int TotalAmount { get; set; }

        public bool IsB2B { get; set; }

        // B2B 
        public int? B2BId { get; set; }

        public string? B2BName { get; set; }



        [Required(ErrorMessage = "A phone number is required")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^([0-9]{10})$", ErrorMessage = "Invalid Phone Number.")]
        public string PhoneNumber { get; set; }


        [Required(ErrorMessage = "Title is required.")]
        public  string Title { get; set; }



        [Required(ErrorMessage = "FirstName is required.")]
        [StringLength(25, MinimumLength = 1, ErrorMessage = "FirstName must be between 1 and 25 characters.")]
        public string FirstName { get; set; }


        [Required(ErrorMessage = "MiddleName is required.")]
        [StringLength(25, MinimumLength = 1, ErrorMessage = "MiddleName must be between 1 and 25 characters.")]
        public string MiddleName { get; set; }


        [Required(ErrorMessage = "LastName is required.")]
        [StringLength(25, MinimumLength = 1, ErrorMessage = "LastName must be between 1 and 25 characters.")]
        public string LastName { get; set; }


        [Required(ErrorMessage = "Date of Birth is required.")]
        public DateTime DOB { get; set; }

        public int Age { get; set; }


        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; }


        //[Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "The email address is not valid.")]
        public string? Email { get; set; }


        // CITY
        public int? CityId { get; set; }

        public string? CityName { get; set; }


        // AREA
        public int? AreaId { get; set; }

        public string? AreaName { get; set; }



        public  string? Address { get; set; }

        public int PaymentId { get; set; }

        public string PaymentName { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        public int? Amount { get; set; }

        public string? ChequeNo { get; set; }

        public DateTime? ChequeDate { get; set; }

        public string? TransactionId { get; set; }


        // DOCTOR
        public int? DoctorId { get; set; }

        public string? DoctorName { get; set; }



        public string? CreatedBy { get; set; }

        //public DateTime CreatedOn { get; set; }

        public bool IsActive { get; set; }


        // service

        [ValidateNever]
        public List<ServiceMapping> ServiceMapping { get; set; } = new();


        //[ValidateNever]
        //public List<PaymentMapping> PaymentMapping { get; set; } = new();
    }

    public class ServiceMapping
    {
        public int ServiceId { get; set; }

        public string ServiceCode { get; set; }

        public string ServiceName { get; set; }

        public int B2BAmount { get; set; }

        public int B2CAmount { get; set; }

        //public bool IsActive { get; set; }

        public int SampleServiceMapId { get; set; }

        //public DateTime CreatedOn { get; set; }

    }


    //public class PaymentMapping
    //{
    //    public int PaymentId { get; set; }


    //    public string PaymentName { get; set; }


    //    public bool IsCash { get; set; }

    //    public bool IsCheque { get; set; }

    //    public bool IsOnline { get; set; }

    //    public bool IsActive { get; set; }
    //}

}
