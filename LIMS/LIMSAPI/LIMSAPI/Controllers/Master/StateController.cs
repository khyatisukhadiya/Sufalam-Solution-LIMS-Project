using LIMSAPI.Helpers;
using LIMSAPI.Models;
using LIMSAPI.Models.Master;
using LIMSAPI.ServiceLayer;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using System.Net;

namespace LIMSAPI.Controllers.MasterController
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StateController : BaseAPIController
    {
        private readonly LIMSServiceInterface _stateSL;

        public StateController(LIMSServiceInterface stateSL, IConfiguration configuration) : base(configuration)
        {
            _stateSL = stateSL;
        }

        [HttpPost]
        public IActionResult AddUpdatedState([FromBody] StateModal stateModal)
        {
            if (stateModal == null)
            {
                return Error("Invalid request payload.");
            }

            // Validate ModelState (DataAnnotations)
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .Select(v => v.Errors[0])
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { success = false, errors = validationErrors });
            }

            var errors = new List<string>();

            // Duplicate check: same name or code in the same country
            var isDuplicate = _stateSL.IsDuplicate(
                table: "state",
                nameCol: "StateName",
                codeCol: "StateCode",
                nameVal: stateModal.StateName,
                codeVal: stateModal.StateCode,
                excludeId: stateModal.StateId,
                idCol: "StateId",
                additionalConditions: new Dictionary<string, object>
                {
                    { "CountryId", stateModal.CountryId }
                });

            if (isDuplicate)
            {
                errors.Add("A state with the same name or code already exists in the selected country.");
            }

            // Return errors if any
            if (errors.Any())
            {
                return BadRequest(new { success = false, errors });
            }

            try
            {
                var result = _stateSL.AddUpdatedState(stateModal);
                var message = stateModal.StateId > 0 ? "State updated successfully." : "State added successfully.";
                return Success(message, result);
            }
            catch (Exception ex)
            {
                return Error("Failed to save state: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }


        [HttpGet]
        public IActionResult GetAllStates([FromQuery] FilterModel filter)
        {

            var states = _stateSL.GetAllStates(filter);

            return Ok(new { data = states });
        }


        [HttpDelete]
        public IActionResult DeleteStateById(int StateId)
        {
            if (StateId <= 0)
            {
                return BadRequest("Invalid State ID.");
            }

            try
            {

                var result = _stateSL.DeleteStateById(StateId);

                if (result == null)
                {
                    return NotFound("State Not Found");
                }

                string status = result.IsActive ? "activated" : "deactivated";
                return Success(status, result);
            }

            catch (Exception ex)
            {
                return Error("Error while state status: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }


        [HttpGet]
        public IActionResult GetStateIsActivate()
        {
            var state = _stateSL.GetState();

            if (state == null)
            {
                return NotFound(new { message = $"Country not found." });
            }

            return Ok(state);
        }

        [HttpGet]
        public IActionResult GetStateById(int StateId)
        {
            var result = _stateSL.GetStateById(StateId);
            return Ok(result);
        }
    }
}
