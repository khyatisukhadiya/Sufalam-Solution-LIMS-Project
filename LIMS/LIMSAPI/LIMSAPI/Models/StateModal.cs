using System.ComponentModel.DataAnnotations;

namespace LIMSAPI.Models
{
    public class StateModal
    {
        public int? StateId { get; set; }


        [Required(ErrorMessage = "State Code is required.")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "State Code must be between 1 and 5 characters.")]
        public string StateCode { get; set; }


        [Required(ErrorMessage = "State Name is required.")]
        [StringLength(15, MinimumLength = 1, ErrorMessage = "State Name must be between 1 and 15 characters.")]
        public string StateName { get; set; }


        [Required(ErrorMessage = "Country is required.")]
        public int? CountryId { get; set; }


        public bool IsActive { get; set; }


        public string? CountryName { get; set; }
    }
}
