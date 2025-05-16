using System.ComponentModel.DataAnnotations;

namespace LIMSAPI.Models.Master
{
    public class AreaModal
    {

        public int? AreaId { get; set; }

        [Required(ErrorMessage = "Area name is required.")]
        [StringLength(15, MinimumLength = 1, ErrorMessage = "Area Name must be between 1 and 15 characters.")]
        public string AreaName { get; set; }


        [Required(ErrorMessage = "Pin code is required.")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "Pin code must be between 1 and 5 characters.")]
        public string PinCode { get; set; }


        [Required(ErrorMessage = "City Id is required.")]
        public int? CityId { get; set; }

        public bool IsActive { get; set; }

        public string? CityName { get; set; }
    }
}
