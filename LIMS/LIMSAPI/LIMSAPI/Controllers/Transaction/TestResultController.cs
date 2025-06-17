using System.Net;
using LIMSAPI.Models;
using LIMSAPI.Models.Transaction;
using LIMSAPI.Models.TransactionModal;
using LIMSAPI.ServiceLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LIMSAPI.Controllers.Transaction
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class TestResultController : BaseAPIController
    {
        public readonly LIMSServiceInterface _sampleSL;

        public TestResultController(LIMSServiceInterface sampleSL, IConfiguration configuration) : base(configuration)
        {
            _sampleSL = sampleSL;
        }

        [HttpPost]
        public IActionResult AddUpdateTestResult([FromBody]TestResultModal testResult)
        {
            if (testResult == null)
            {
                return Error("Invalid request payload input");
            }


            if (!ModelState.IsValid)
            {
                var validationError = ModelState.Values
                    .Select(v => v.Errors[0])
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { success = false, errors = validationError });
            }

            var errors = new List<string>();


            if (errors.Any())
            {
                return BadRequest(new { Success = false, errors });
            }

            try
            {
                bool isUpdate = testResult.TestResultId > 0;
                var result = _sampleSL.AddUpdateTestResult(testResult);
                var message = isUpdate ? "TestResult Update Successfully" : "TestResult Add Successfully.";
                return Success(message, result);
            }
            catch (Exception ex)
            {
                return Error("Failed to save data" + ex.Message, HttpStatusCode.InternalServerError);
            }

        }

        //[HttpGet]
        //public IActionResult GetTestResultByFilter([FromQuery]FilterModel filterModel)
        //{
        //    try
        //    {
        //        var results = _sampleSL.GetTestResultByFilter(filterModel);
        //        return Success("Test Results retrieved successfully", results);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Error("Failed to retrieve test results: " + ex.Message, HttpStatusCode.InternalServerError);
        //    }
        //}

    }
}
