using System.ComponentModel.DataAnnotations;

namespace LIMSAPI.Models
{
    public class DoctorModal
    {

        [Required(ErrorMessage ="Doctor Id is required.")]
        public int DoctorId { get; set; }



        [Required(ErrorMessage = "Doctor Code is required.")]
        [StringLength(5, MinimumLength =1, ErrorMessage = "Doctor Code must be between 1 and 5 characters.")]
        public string DoctorCode { get; set; }



        [Required(ErrorMessage = "Doctor Name is required.")]
        [StringLength(25, MinimumLength = 1, ErrorMessage = "Doctor Name must be between 1 and 25 characters.")]
        public string DoctorName { get; set; }



        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "The email address is not valid.")]
        public string Email { get; set; }


        [Required(ErrorMessage = "A phone number is required")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^([0-9]{10})$", ErrorMessage = "Invalid Phone Number.")]
        public string PhoneNumber { get; set; }

        public bool IsActive { get; set; }
    }
}
