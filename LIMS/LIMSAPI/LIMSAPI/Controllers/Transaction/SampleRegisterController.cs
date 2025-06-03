using LIMSAPI.Models.Master;
using LIMSAPI.Models;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LIMSAPI.ServiceLayer;
using LIMSAPI.Models.TransactionModal;
using Azure.Core;
using System.Reflection;

namespace LIMSAPI.Controllers.Transaction
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class SampleRegisterController : BaseAPIController
    {
        public readonly LIMSServiceInterface _sampleSL;

        public SampleRegisterController(LIMSServiceInterface sampleSL, IConfiguration configuration) : base(configuration)
        {
            _sampleSL = sampleSL;
        }



        [HttpPost]

        public IActionResult AddUpdateSampleRegister([FromBody]SampleRegister sampleRegister)
        {

            if (sampleRegister == null)
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

            //var isDuplicate = _sampleSL.IsDuplicate(
            //    table: "area",
            //    nameCol: "AreaName",
            //    codeCol: "PinCode",
            //    nameVal: areaModal.AreaName,
            //    codeVal: areaModal.PinCode,
            //    excludeId: areaModal.AreaId,
            //    idCol: "AreaId",
            //    additionalConditions: new Dictionary<string, object>
            //    {
            //        {"CityId", areaModal.CityId},
            //    });

            //if (isDuplicate)
            //{
            //    errors.Add("A area with the same name or PinCode already exists in the selected city.");
            //}

            if (errors.Any())
            {
                return BadRequest(new { Success = false, errors });
            }

            try
            {
                bool isUpdate = sampleRegister.SampleRegisterId > 0;

                var result = _sampleSL.AddUpdateSampleRegister(sampleRegister);
                var message = isUpdate ? "SampleRegister Update Successfully" : "SampleRegister Add Successfully.";
                return Success(message, result);
            }
            catch (Exception ex)
            {
                return Error("Failed to save data" + ex.Message, HttpStatusCode.InternalServerError);
            }

        }

        //[HttpDelete]
        //public IActionResult DeleteSampleRegisterById(int SampleRegisterId)
        //{
        //    var result = _sampleSL.DeleteSampleRegisterById(SampleRegisterId);
        //    return Ok(new { data = result });
        //}


        //[HttpGet]
        //public IActionResult GetSampleByFilter([FromQuery] FilterModel filterModel)
        //{
        //    var result = _sampleSL.GetSampleByFilter(filterModel);
        //    return Ok(new { data = result });
        //} 
        
        [HttpGet]
        public IActionResult GetSampleByFilter()
        {
            var result = _sampleSL.GetSampleByFilter();
            return Ok(new { data = result });
        }


        [HttpGet]
        public IActionResult GetSampleRegisterById(int SampleRegisterId)
        {
            var result = _sampleSL.GetSampleRegisterById(SampleRegisterId);
            return Ok(result);
        }


    

        [HttpDelete]
        public IActionResult DeleteSampleServiceMapId(int SampleServiceMapId)
        {
            var result = _sampleSL.DeleteSampleServiceMapId(SampleServiceMapId);
            return Ok(new { data = result });
        }


    }
}
