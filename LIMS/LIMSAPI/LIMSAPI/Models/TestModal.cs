using System.ComponentModel.DataAnnotations;

namespace LIMSAPI.Models
{
    public class TestModal
    {
        public int TestId { get; set; }


        [Required(ErrorMessage = "Test name is required.")]
        [StringLength(25, MinimumLength = 1, ErrorMessage = "Test Name must be between 1 and 25 characters.")]
        public string TestName { get; set; }



        [Required(ErrorMessage = "Test code is required.")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "Test Code must be between 1 to 5 character.")]
        public string TestCode { get; set; }


        public bool IsActive { get; set; }
    }
}
