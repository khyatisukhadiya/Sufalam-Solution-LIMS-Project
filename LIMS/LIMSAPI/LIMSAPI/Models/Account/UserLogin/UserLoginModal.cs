using System.ComponentModel.DataAnnotations;
using LIMSAPI.Models.Account.UserRegistration;

namespace LIMSAPI.Models.Account.UserLogin
{
    public class UserLoginModal
    {

        //[Display(Name = "Email")]
        //[RegularExpression(".+@.+\\..+", ErrorMessage = "Please Enter Correct Email Address")]
        //public string Email { get; set; }

        public string UserName { get; set; }



        [Required(ErrorMessage = "Please Enter Password")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }



        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        //public List<UserRegistrationMapping> UserRegistrations { get; set; } = new List<UserRegistrationMapping>();
    }

    //public class UserRegistrationMapping
    //{
    //    public int UserId { get; set; }

    //    public string UserName { get; set; }

    //    public string FullName { get; set; }

    //    public string Email { get; set; }

    //    public string PhoneNumber { get; set; }

    //    public string Password { get; set; }

    //    public string ConfirmPassword { get; set; }

    //    public char Gender { get; set; }

    //    public DateOnly DOB { get; set; }
    //}


}
