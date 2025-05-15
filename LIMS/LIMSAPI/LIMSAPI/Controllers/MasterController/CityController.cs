using System.Net;
using LIMSAPI.Models.Master;
using LIMSAPI.ServiceLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LIMSAPI.Controllers.MasterController
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class CityController : BaseAPIController
    {
        public readonly LIMSServiceInterface _citySL;

        public CityController(LIMSServiceInterface citySL, IConfiguration configuration) : base(configuration)
        {
            _citySL = citySL;
        }

        [HttpPost]
        public IActionResult AddUpdatedCity([FromBody] CityModal cityModal)
        {
            if(cityModal == null)
            {
                return Error("Invalid request payload");
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

            var isDuplicate = _citySL.IsDuplicate(
                table: "city",
                nameCol: "CityName",
                codeCol: "CityCode",
                nameVal: cityModal.CityName,
                codeVal: cityModal.CityCode,
                excludeId: cityModal.CityId,
                idCol: "CityId",
                additionalConditions: new Dictionary<string, object>
                {
                    {"StateId", cityModal.StateId }
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
                var result = _citySL.AddUpdatedCity(cityModal);
                var message = cityModal.CityId > 0 ? "City update successfully." : "City added successfully.";
                return Success(message, result);
            }
            catch(Exception ex)
            {
                return Error("Failed to save data" + ex.Message, HttpStatusCode.InternalServerError);
            }
        }


        [HttpGet]
        public IActionResult GetAllCities([FromQuery] FilterModel filter)
        {
            var result = _citySL.GetAllCities(filter);
            return Ok( new { data = result});
        }

        [HttpDelete]
        public IActionResult DeleteCityById(int CityId)
        {
            var result = _citySL.DeleteCityById(CityId);
            string status = result.IsActive ? "activated" : "deactivated";
            return Success(status,result);
        }

        [HttpGet]
        public IActionResult GetCityIsActive()
        {
            var result = _citySL.GetCityIsActive();
            return Ok(result);
        }

        [HttpGet]
        public IActionResult GetCityById(int CityId)
        {
            var result = _citySL.GetCityById(CityId);
            return Ok(result);
        }


    }
}
