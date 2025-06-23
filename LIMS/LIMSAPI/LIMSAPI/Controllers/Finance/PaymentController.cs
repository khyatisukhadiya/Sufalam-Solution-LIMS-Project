using System.Net;
using LIMSAPI.Models;
using LIMSAPI.Models.FinanceModal;
using LIMSAPI.RepositryLayer;
using LIMSAPI.ServiceLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LIMSAPI.Controllers.Finance
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class PaymentController : BaseAPIController
    {
        public LIMSServiceInterface _payment;

        public PaymentController(LIMSServiceInterface payment, IConfiguration configuration) : base(configuration)
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

            var isDuplicate = _payment.IsDuplicate(
                table: "payment",
                nameCol: "PaymentName",
                codeCol: "PaymentName",
                nameVal: paymentModal.PaymentName,
                codeVal: paymentModal.PaymentName,
                excludeId: paymentModal.PaymentId,
                idCol: "PaymentId",
                additionalConditions: new Dictionary<string, object>());
            ;
            if (isDuplicate)
            {
                errors.Add("A payment name already exists.");
            }

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


        [HttpGet]
        public IActionResult GetPaymentByFilter([FromQuery ]FilterModel filterModel)
        {
            var result = _payment.GetPaymentByFilter(filterModel);
            return Ok( new { data = result });
        }


        [HttpGet]
        public IActionResult GetPaymentById(int PaymentId)
        {
            var result = _payment.GetPaymentById(PaymentId);
            return Ok(result);
        }

        [HttpDelete]
        public  IActionResult DeletePaymentById(int PaymentId)
        {
            var result = _payment.DeletePaymentById(PaymentId);
            string status = result.IsActive ? "activated" : "deactivated";
            return Success(status, result);
        }

        [HttpGet]
        public IActionResult GetPaymentIsActive()
        {
            var result = _payment.GetPaymentIsActive();
            return Ok(result);
        }
    }
}
