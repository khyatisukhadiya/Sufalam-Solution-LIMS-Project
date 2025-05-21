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


        // B2B 
        public int? B2BId { get; set; }

        public string? B2BName { get; set; }




        public string PhoneNumber { get; set; }

        public string Title { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public DateTime DOB { get; set; }

        public int Age { get; set; }

        public string Gender { get; set; }

        public string Email { get; set; }


        // CITY
        public int? CityId { get; set; }

        public string? CityName { get; set; }


        // AREA
        public int? AreaId { get; set; }

        public string? AreaName { get; set; }



        public string Address { get; set; }


        // DOCTOR
        public int? DoctorId { get; set; }

        public string? DoctorName { get; set; }



        public bool IsActive { get; set; }


        // service

        [ValidateNever]
        public List<ServiceModal> service { get; set; } = new List<ServiceModal>();
    }
}
