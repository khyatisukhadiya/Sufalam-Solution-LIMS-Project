using System.Net;
using LIMSAPI.ServiceLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using LIMSAPI.Models.Master;

namespace LIMSAPI.Controllers.MasterController
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class ServiceController : BaseAPIController
    {
        private readonly LIMSServiceInterface _serviceSL;

        public ServiceController(LIMSServiceInterface serviceSL, IConfiguration configuration) : base(configuration)
        {
            _serviceSL = serviceSL;
        }


        [HttpPost]
        public IActionResult AddUpdatedService([FromBody] ServiceRequestDto request)
        {

            if (request == null || request.Service == null)
            {
                return Error("Invalid request payload");
            }

            var serviceModal = request.Service;
            var testModals = request.Tests;

            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .Select(v => v.Errors[0])
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { success = false, errors = validationErrors, data = new { service = serviceModal, test = testModals } });
            }

            var errors = new List<string>();

            var isDuplicate = _serviceSL.IsDuplicate(
                table: "service",
                nameCol: "ServiceName",
                codeCol: "ServiceCode",
                nameVal: serviceModal.ServiceName,
                codeVal: serviceModal.ServiceCode,
                excludeId: serviceModal.ServiceId,
                idCol: "ServiceId");

            if (isDuplicate)
            {
                errors.Add("A Service with the same name or code already exists in the selected Service.");
            }

            if (errors.Any())
            {
                return BadRequest(new { Success = false, errors });
            }

            try
            {
                var result = _serviceSL.AddUpdatedServiceModal(serviceModal, testModals);
                var message = serviceModal.ServiceId > 0 ? "Service updated successfully." : "Service added successfully.";
                return Success(message, result);
            }
            catch (Exception ex)
            {
                return Error("Failed to save data: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }


        [HttpGet]
        public IActionResult GetServiceByFilter([FromQuery] FilterModel filterModel)
        {

            var services = _serviceSL.GetServiceByFilter(filterModel);

            return Ok(new { data = services });
        }

        [HttpDelete]
        public IActionResult DeleteServiceById(int ServiceId)
        {

            try
            {

                if (ServiceId <= 0)
                {
                    return BadRequest("Invalid test ID.");
                }


                var result = _serviceSL.DeleteServiceById(ServiceId);

                if (result == null)
                {
                    return NotFound("Service Not Found");
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
        public IActionResult GetServiceIsActive()
        {
            var services = _serviceSL.GetServiceIsActive();

            if (services == null)
            {
                return NotFound(new { message = $"Service not found." });
            }

            return Ok(services);
        }


        [HttpGet]
        public IActionResult GetServiceById(int ServiceId)
        {

            var services = _serviceSL.GetServiceById(ServiceId);
            return Ok(services);

        }



        [HttpDelete]
        public IActionResult DeleteServiceMapTestById(int ServiceTestId)
        {
            var result = _serviceSL.DeleteServiceMapTestById(ServiceTestId);

            if (result == null)
            {
                return NotFound("Service Not Found");
            }

            string status = result.IsActive ? "activated" : "deactivated";
            return Success(status, result);
        }
    }
}
