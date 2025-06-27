using System.ComponentModel.DataAnnotations;

namespace LIMSAPI.Models.Account.UserLogin
{
    public class UserLoginModal
    {
        [Required(ErrorMessage = " Please Enter Email")]
        [EmailAddress]
        [Display(Name = "Email")]
        [RegularExpression(".+@.+\\..+", ErrorMessage = "Please Enter Correct Email Address")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Please Enter Password")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }



        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
