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

        //[HttpPost]
        //public IActionResult AddUpdateTestResult([FromBody]TestResultModal testResult)
        //{
        //    if (testResult == null)
        //    {
        //        return Error("Invalid request payload input");
        //    }


        //    if (!ModelState.IsValid)
        //    {
        //        var validationError = ModelState.Values
        //            .Select(v => v.Errors[0])
        //            .Select(e => e.ErrorMessage)
        //            .ToList();

        //        return BadRequest(new { success = false, errors = validationError });
        //    }

        //    var errors = new List<string>();


        //    if (errors.Any())
        //    {
        //        return BadRequest(new { Success = false, errors });
        //    }

        //    try
        //    {
        //        bool isUpdate = testResult.TestResultId > 0;
        //        var result = _sampleSL.AddUpdateTestResult(testResult);
        //        var message = isUpdate ? "TestResult Update Successfully" : "TestResult Add Successfully.";
        //        return Success(message, result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Error("Failed to save data" + ex.Message, HttpStatusCode.InternalServerError);
        //    }

        //}

       
        [HttpPost]
        public IActionResult AddUpdateTestResult([FromBody]TestResultDto testResults)
        {

            if (testResults == null)
            {
                return Error("Invalid or missing data.", HttpStatusCode.BadRequest);
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

                List<TestResultModal> testResultModals = testResults.SampleRegister.SelectMany(sampleRegister => sampleRegister.Services.SelectMany(service => service.Tests.Select(test => new TestResultModal
                {
                    SampleRegisterId = sampleRegister.SampleRegisterId,
                    ServiceId = service.ServiceId,
                    ServiceName = service.ServiceName,
                    TestId = test.TestId,
                    TestName = test.TestName,
                    ResultValue = test.ResultValue,
                    ValidationStatus = test.ValidationStatus,
                    ValidateBy = test.ValidateBy,
                    CreatedBy = test.CreatedBy,
                    IsActive = test.IsActive
                }))).ToList();

                var result = _sampleSL.AddUpdateTestResults(testResults);
                var message = "Data added successfully";
                return Success(message, result);
            }
            catch (Exception ex)
            {
                return Error("Failed to save data: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]

        public IActionResult GetTestResultById(int SampleRegisterId)
        {
            var result = _sampleSL.GetTestResultById(SampleRegisterId);
            return Ok(result);
        }


    }
}
