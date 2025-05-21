using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LIMSAPI.ServiceLayer;
using LIMSAPI.Models.Master;
using LIMSAPI.Models;

namespace LIMSAPI.Controllers.MasterController
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class CountryController : BaseAPIController
    {
        private readonly LIMSServiceInterface _countrySL;

        public CountryController(LIMSServiceInterface countrySL, IConfiguration configuration) : base(configuration)
        {
            _countrySL = countrySL;
        }


        [HttpPost]
        public IActionResult AddUpdatedCountry([FromBody] CountryModal countryModal)
        {
            if (countryModal == null)
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

            var isDuplicate = _countrySL.IsDuplicate(
                 table: "country",
                 nameCol: "CountryName",
                 codeCol: "CountryCode",
                 nameVal: countryModal.CountryName,
                 codeVal: countryModal.CountryCode,
                 excludeId: countryModal.CountryId,
                 idCol: "CountryId");

            if (isDuplicate)
            {
                errors.Add("A country with the same name or code already exists.");
            }

            if (errors.Any())
            {
                return BadRequest(new { errors });
            }

            try
            {
                var result = _countrySL.AddUpdatedCountry(countryModal);
                string message = countryModal.CountryId > 0 ? "Country updated successfully." : "Country added successfully.";
                return Success(message, result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message, HttpStatusCode.InternalServerError);
            }
        }


        [HttpGet]
        public IActionResult GetCountriesByFilter([FromQuery] FilterModel filter)
        {
       
            var countries = _countrySL.GetAllCountries(filter);

            return Ok(new { data = countries });
        }


        [HttpDelete]
        public IActionResult DeleteCountryById(int countryId)
        {
          
            try
            {

                if (countryId <= 0)
                {
                    return BadRequest("Invalid country ID.");
                }


                var result = _countrySL.DeleteCountryById(countryId);

                if (result == null)
                {
                    return NotFound("Country Not Found");
                }

                string status = result.IsActive ? "activated" : "deactivated";
                return Success(status, result);
            }

            catch (Exception ex)
            {
                return Error("Error while country status: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public IActionResult GetCountryIsActive()
        {
            var country = _countrySL.GetCountry(); 

            if (country == null)
            {
                return NotFound(new { message = $"Country not found." });
            }

            return Ok(country);
        }

        [HttpGet]
        public IActionResult GetCountryById(int CountryId)
        {
            
                var country = _countrySL.GetCountryById(CountryId);
                return Ok(country);
         
        }

    }
}
