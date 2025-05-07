using LIMSAPI.Models;
using System.Net;
using LIMSAPI.RepositryLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LIMSAPI.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class TestController :  BaseAPIController
    {
        public LIMSRepositryInterface _testSL;

        public TestController(LIMSRepositryInterface testSL, IConfiguration configuration) : base(configuration) 
        {
            _testSL = testSL;
        }


        [HttpPost]
        public IActionResult AddUpdatedTest([FromBody] TestModal testModal)
        {
            if (testModal == null)
            {
                return Error("Invalid data.");

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

            var isDuplicate = _testSL.IsDuplicate(
                 table: "test",
                 nameCol: "TestName",
                 codeCol: "TestCode",
                 nameVal: testModal.TestName,
                 codeVal: testModal.TestCode,
                 excludeId: testModal.TestId,
                 idCol: "TestId");

            if (isDuplicate)
            {
                errors.Add("A test with the same name or code already exists.");
            }

            if (errors.Any())
            {
                return BadRequest(new { errors });
            }

            try
            {
                var result = _testSL.AddUpdatedTest(testModal);
                string message = testModal.TestId > 0 ? "Test updated successfully." : "Test added successfully.";
                return Success(message, result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message, HttpStatusCode.InternalServerError);
            }
        }


        [HttpGet]
        public IActionResult GetTestByFilter([FromQuery] FilterModel filterModel)
        {

            var test = _testSL.GetTestByFilter(filterModel);

            return Ok(new { data = test });
        }


        [HttpDelete]
        public IActionResult DeleteTestById(int TestId)
        {

            try
            {

                if (TestId <= 0)
                {
                    return BadRequest("Invalid test ID.");
                }


                var result = _testSL.DeleteTestById(TestId);

                if (result == null)
                {
                    return NotFound("Test Not Found");
                }

                string status = result.IsActive ? "activated" : "deactivated";
                return Success(status, result);
            }

            catch (Exception ex)
            {
                return Error("Error while test status: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public IActionResult GetTestIsActive()
        {
            var test = _testSL.GetTestIsActive();

            if (test == null)
            {
                return NotFound(new { message = $"Test not found." });
            }

            return Ok(test);
        }


        [HttpGet]
        public IActionResult GetTestById(int TestId)
        {

            var test = _testSL.GetTestById(TestId);
            return Ok(test);

        }
    }
}
