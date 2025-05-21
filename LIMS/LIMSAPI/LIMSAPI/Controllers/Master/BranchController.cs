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
    public class BranchController : BaseAPIController
    {
        private readonly LIMSServiceInterface _branchSL;

        public BranchController(LIMSServiceInterface branchSL, IConfiguration configuration) : base(configuration) 
        {
            _branchSL = branchSL;
        }


        [HttpPost]
        public IActionResult AddUpdatedBranch([FromBody] BranchModal branchModal)
        {
            if (branchModal == null)
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

            var isDuplicate = _branchSL.IsDuplicate(
                 table: "branch",
                 nameCol: "BranchName",
                 codeCol: "BranchCode",
                 nameVal: branchModal.BranchName,
                 codeVal: branchModal.BranchCode,
                 excludeId: branchModal.BranchId,
                 idCol: "BranchId");

            if (isDuplicate)
            {
                errors.Add("A branch with the same name or code already exists.");
            }

            if (errors.Any())
            {
                return BadRequest(new { errors });
            }

            try
            {
                var result = _branchSL.AddUpdatedBranch(branchModal);
                string message = branchModal.BranchId > 0 ? "Branch updated successfully." : "Branch added successfully.";
                return Success(message, result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message, HttpStatusCode.InternalServerError);
            }
        }


        [HttpGet]
        public IActionResult GetBranchesByFilter([FromQuery] FilterModel filterModel)
        {

            var branches = _branchSL.GetBranches(filterModel);

            return Ok(new { data = branches });
        }


        [HttpDelete]
        public IActionResult DeleteBranchById(int BranchId)
        {

            try
            {

                if (BranchId <= 0)
                {
                    return BadRequest("Invalid branch ID.");
                }


                var result = _branchSL.DeleteBranchById(BranchId);

                if (result == null)
                {
                    return NotFound("Branch Not Found");
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
        public IActionResult GetBranchIsActive()
        {
            var branch = _branchSL.GetBranchIsActive();

            if (branch == null)
            {
                return NotFound(new { message = $"Branch not found." });
            }

            return Ok(branch);
        }

        [HttpGet]
        public IActionResult GetBranchById(int BranchId)
        {

            var branch = _branchSL.GetBranchById(BranchId);
            return Ok(branch);

        }

    }
}
