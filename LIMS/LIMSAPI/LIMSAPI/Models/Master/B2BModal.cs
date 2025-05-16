using System.ComponentModel.DataAnnotations;

namespace LIMSAPI.Models.Master
{
    public class B2BModal
    {
        public int B2BId { get; set; }


        [Required(ErrorMessage = "B2B name is required.")]
        [StringLength(15, MinimumLength = 1, ErrorMessage = "B2B Name must be between 1 and 15 characters.")]
        public string B2BName { get; set; }



        [Required(ErrorMessage = "B2B code is required.")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "B2B Code must be between 1 to 5 character.")]
        public string B2BCode { get; set; }


        public bool IsActive { get; set; }
    }
}
