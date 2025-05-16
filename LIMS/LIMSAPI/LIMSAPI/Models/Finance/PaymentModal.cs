using System.ComponentModel.DataAnnotations;

namespace LIMSAPI.Models.FinanceModal
{
    public class PaymentModal
    {


        public int PaymentId { get; set; }

        [Required(ErrorMessage = "payment Name is required.")]
        [StringLength(15, MinimumLength = 1, ErrorMessage = "payment name must be between 1 and 15 characters.")]
        public string PaymentName { get; set; }


        public bool IsCash { get; set; }


        public bool IsCheque { get; set; }


        public bool IsOnline { get; set; }


        public bool IsActive { get; set; }

    }
}
