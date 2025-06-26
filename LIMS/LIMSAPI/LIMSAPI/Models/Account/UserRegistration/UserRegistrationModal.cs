using System.ComponentModel.DataAnnotations;

namespace LIMSAPI.Models.Account.UserRegistration
{
    public class UserRegistrationModal
    {

        public int UserId { get; set; }



        [Required(ErrorMessage = "Please Enter UserName")]
        [Display(Name = "UserName")]
        public string UserName { get; set; }



        [Required(ErrorMessage = "Please Enter FullName")]
        [Display(Name = "FullName")]
        public string FullName { get; set; }



        [Required(ErrorMessage =" Please Enter Email")]
        [EmailAddress]
        [Display(Name ="Email")]
        [RegularExpression(".+@.+\\..+", ErrorMessage = "Please Enter Correct Email Address")]
        public string Email { get; set; }



        [Required(ErrorMessage = "Please Enter Mobile No")]
        [Display(Name = "PhoneNumber")]
        [StringLength(10, ErrorMessage = "The Mobile must contains 10 characters", MinimumLength = 10)]
        public string PhoneNumber { get; set; }



        [Required(ErrorMessage = "Please Enter Password")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }




        [Required(ErrorMessage = "Please Enter Confirm Password")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }



        [Required(ErrorMessage = "Please Enter Gender")]
        [Display(Name = "Gender")]
        public char Gender { get; set; }



        [Required(ErrorMessage = "Please Enter DOB")]
        [Display(Name = "DOB")]
        public DateOnly DOB { get; set; }


    }
}
