using System.ComponentModel.DataAnnotations;

namespace LIMSAPI.Models.Master
    {
        public class CountryModal
        {
            public int CountryId { get; set; }


            [Required(ErrorMessage = "Country name is required.")]
            [StringLength(15, MinimumLength = 1, ErrorMessage = "Country Name must be between 1 and 15 characters.")]
            public string CountryName { get; set; }



            [Required(ErrorMessage = "Country code is required.")]
            [StringLength(5, MinimumLength = 1, ErrorMessage ="Country Code must be between 1 to 5 character.")]
            public string CountryCode { get; set; }


            public bool IsActive { get; set; }
        }
    }
