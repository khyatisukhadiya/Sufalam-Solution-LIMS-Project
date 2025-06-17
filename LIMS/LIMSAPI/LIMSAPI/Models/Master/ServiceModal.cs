
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LIMSAPI.Models.Master
{
    public class ServiceModal
    {
        public int ServiceId { get; set; }


        [Required(ErrorMessage = "Service Code is required.")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "Service Code must be between 1 and 5 characters.")]
        public string ServiceCode { get; set; }


        [Required(ErrorMessage = "Service Name is required.")]
        [StringLength(30, MinimumLength = 1, ErrorMessage = "Service Name must be between 1 and 30 characters.")]
        public string ServiceName { get; set; }



        //[Required(ErrorMessage = "B2BAmount is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "B2B Amount is Greater than Zero.")]
        public int B2BAmount { get; set; }


        //[Required(ErrorMessage = "B2CAmount is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "B2C Amount is Greater than zero.")]
        public int B2CAmount { get; set; }

        public bool IsActive { get; set; }



        [ValidateNever]
        public List<TestModal> Test { get; set; } = new List<TestModal>();



        
    }
}
