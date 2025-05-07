using System.ComponentModel.DataAnnotations;

namespace LIMSAPI.Models
{
    public class AreaModal
    {

        public int? AreaId { get; set; }

        [Required(ErrorMessage = "Area name is required.")]
        [StringLength(15, MinimumLength = 1, ErrorMessage = "Area Name must be between 1 and 15 characters.")]
        public string AreaName { get; set; }


        [Required(ErrorMessage = "Area code is required.")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "Area code must be between 1 and 5 characters.")]
        public string AreaCode { get; set; }


        [Required(ErrorMessage = "City Id is required.")]
        public int? CityId { get; set; }

        public bool IsActive { get; set; }

        public string? CityName { get; set; }
    }
}
