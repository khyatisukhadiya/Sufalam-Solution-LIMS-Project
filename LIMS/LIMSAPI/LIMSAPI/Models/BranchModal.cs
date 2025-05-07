using System.ComponentModel.DataAnnotations;

namespace LIMSAPI.Models
{
    public class BranchModal
    {

        public int BranchId { get; set; }


        [Required(ErrorMessage = "Branch name is required.")]
        [StringLength(15, MinimumLength = 1, ErrorMessage = "Branch Name must be between 1 and 15 characters.")]
        public string BranchName { get; set; }



        [Required(ErrorMessage = "Branch code is required.")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "Branch Code must be between 1 to 5 character.")]
        public string BranchCode { get; set; }


        public bool IsActive { get; set; }
    }
}
