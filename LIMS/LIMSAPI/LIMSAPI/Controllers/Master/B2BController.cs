using System.Net;
using LIMSAPI.ServiceLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LIMSAPI.Models.Master;
using LIMSAPI.Models;

namespace LIMSAPI.Controllers.MasterController
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class B2BController : BaseAPIController
    {
        private readonly LIMSServiceInterface _b2bSL;

        public B2BController(LIMSServiceInterface b2bSL, IConfiguration configuration) : base(configuration)
        {
            _b2bSL = b2bSL;
        }


        [HttpPost]
        public IActionResult AddUpdatedB2B([FromBody] B2BModal b2BModal)
        {
            if (b2BModal == null)
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

            var isDuplicate = _b2bSL.IsDuplicate(
                 table: "b2b",
                 nameCol: "B2BName",
                 codeCol: "B2BCode",
                 nameVal: b2BModal.B2BName,
                 codeVal: b2BModal.B2BCode,
                 excludeId: b2BModal.B2BId,
                 idCol: "B2BId");

            if (isDuplicate)
            {
                errors.Add("A b2b with the same name or code already exists.");
            }

            if (errors.Any())
            {
                return BadRequest(new { errors });
            }

            try
            {
                var result = _b2bSL.AddUpdatedB2B(b2BModal);
                string message = b2BModal.B2BId > 0 ? "B2B updated successfully." : "B2B added successfully.";
                return Success(message, result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message, HttpStatusCode.InternalServerError);
            }
        }


        [HttpGet]
        public IActionResult GetB2BByFilter([FromQuery] FilterModel filterModel)
        {

            var b2b = _b2bSL.GetB2Bs(filterModel);

            return Ok(new { data = b2b });
        }

        [HttpDelete]
        public IActionResult DeleteB2BById(int B2BId)
        {

            try
            {

                if (B2BId <= 0)
                {
                    return BadRequest("Invalid b2b ID.");
                }


                var result = _b2bSL.DeleteB2BById(B2BId);

                if (result == null)
                {
                    return NotFound("B2B Not Found");
                }

                string status = result.IsActive ? "activated" : "deactivated";
                return Success(status, result);
            }

            catch (Exception ex)
            {
                return Error("Error while branch status: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }


        [HttpGet]
        public IActionResult GetB2BIsActive()
        {
            var b2b = _b2bSL.GetB2BIsActive();

            if (b2b == null)
            {
                return NotFound(new { message = $"B2B not found." });
            }

            return Ok(b2b);
        }


        [HttpGet]
        public IActionResult GetB2BsById(int B2BId)
        {

            var b2b = _b2bSL.GetB2BById(B2BId);
            return Ok(b2b);

        }


    }
}
