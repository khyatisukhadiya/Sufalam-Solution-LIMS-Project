using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace LIMSAPI.Models
{
    public class CityModal
    {

        public int? CityId { get; set; }

        [Required(ErrorMessage = "City Code is required.")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "City Code must be between 1 and 5 characters.")]
        public string CityCode { get; set; }


        [Required(ErrorMessage = "City Name is required.")]
        [StringLength(15, MinimumLength = 1, ErrorMessage = "City Code must be between 1 and 15 characters.")]
        public string CityName { get; set; }


        [Required(ErrorMessage = "State is required.")]
        public int? StateId { get; set; }


        public bool IsActive { get; set; }


        public string? StateName { get; set; }
    }
}
