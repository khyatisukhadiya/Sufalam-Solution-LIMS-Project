using System.Net;
using LIMSAPI.Models;
using LIMSAPI.ServiceLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LIMSAPI.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class AreaController : BaseAPIController
    {
        public readonly LIMSServiceInterface _areaSL;

        public AreaController(LIMSServiceInterface areaSL, IConfiguration configuration) : base(configuration)
        {
            _areaSL = areaSL;
        }

        [HttpPost]

        public IActionResult AddUpdatedArea(AreaModal areaModal)
        {
            if(areaModal == null)
            {
                return Error("Invalid request payload.");
            }

            if (!ModelState.IsValid)
            {
                var validationError = ModelState.Values
                    .Select(v => v.Errors[0])
                    .Select(e => e.ErrorMessage)
                    .ToList();
            }

            var errors = new List<string>();

            var isDuplicate = _areaSL.IsDuplicate(
                table: "area",
                nameCol: "AreaName",
                codeCol: "AreaCode",
                nameVal: areaModal.AreaName,
                codeVal: areaModal.AreaCode,
                excludeId: areaModal.AreaId,
                idCol: "AreaId",
                additionalConditions: new Dictionary<string, object>
                {
                    {"CityId", areaModal.CityId},
                });

            if (isDuplicate)
            {
                errors.Add("A city with the same name or code already exists in the selected city.");
            }

            if (errors.Any())
            {
                return BadRequest(new { Success = false, errors });
            }

            try
            {
                var result = _areaSL.AddUpdatedArea(areaModal);
                var message = areaModal.AreaId > 0 ? "Area Update Successfully" : "Area Add Successfully.";
                return Success(message, result);
            }
            catch (Exception ex)
            {
                return Error("Failed to save data" + ex.Message, HttpStatusCode.InternalServerError);
            }

        }


        [HttpGet]
        public IActionResult GetAllArea([FromQuery] FilterModel filterModel)
        {
            var result = _areaSL.GetAllArea(filterModel);
            return Ok(new { data = result });
        }

        [HttpDelete]
        public IActionResult DeleteAreaById(int areaId)
        {
            var result = _areaSL.DeleteAreaById(areaId);
            return Ok( new { data = result});
        }

        [HttpGet]
        public IActionResult GetAreaIsActive()
        {
            var result = _areaSL.GetAreaIsActive();
            return Ok( new { data = result });
        }


        [HttpGet]
        public IActionResult GetAreaById(int AreaId)
        {
            var result = _areaSL.GetAreaById(AreaId);
            return Ok(new { data = result });
        }
    }
}
