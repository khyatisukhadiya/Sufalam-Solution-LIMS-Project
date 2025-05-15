using System.Net;
using LIMSAPI.Controllers.MasterController;
using LIMSAPI.Models.FinanceModal;
using LIMSAPI.Models.Master;
using LIMSAPI.RepositryLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LIMSAPI.Controllers.Finance
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : BaseAPIController
    {
        public LIMSRepositryInterface _payment;

        public PaymentController(LIMSRepositryInterface payment, IConfiguration configuration) : base(configuration)
        {
            _payment = payment;
        }



        [HttpPost]
        public IActionResult AddUpdatedPayment(PaymentModal paymentModal)
        {
            if(paymentModal == null)
            {
                return Error("Invalid Payload request.");
            }


            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .Select(v => v.Errors[0])
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { success = false, errors = validationErrors });
            }


            var errors = new List<string>();

            //var isDuplicate = _payment.IsDuplicate(
            //    table: "payment",
            //    nameCol: "PaymentName",
            //    codeCol : "",
            //    nameVal: paymentModal.PaymentName,
            //    codeVal : paymentModal.,
            //    excludeId: paymentModal.PaymentId,
                idCol: "PaymentId",

            //if (isDuplicate)
            //{
            //    errors.Add("A test with the same name or code already exists.");
            //}

            if (errors.Any())
            {
                return BadRequest(new { errors });
            }


            try
            {
                var payment = _payment.AddUpdatedPayment(paymentModal);
                string message = paymentModal.PaymentId > 0 ? "Update Payment Successfully" : "Add Payment Successfully";
                return Success(message, payment);
            }
            catch(Exception ex)
            {
                return Error(ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}
