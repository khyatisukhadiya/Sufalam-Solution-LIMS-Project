using System.Net;
using LIMSAPI.Models;
using LIMSAPI.RepositryLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LIMSAPI.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class DoctorController : BaseAPIController
    {
        public LIMSRepositryInterface _doctorSL;

        public DoctorController(LIMSRepositryInterface doctorSL, IConfiguration configuration)  : base(configuration) 
        {
            _doctorSL = doctorSL;
        }

        [HttpPost]
        public IActionResult AddUpdatedDoctor(DoctorModal doctorModal)
        {
            if (doctorModal == null)
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

            var isDuplicate = _doctorSL.IsDuplicate(
                 table: "doctor",
                 nameCol: "DoctorName",
                 codeCol: "DoctorCode",
                 nameVal: doctorModal.DoctorName,
                 codeVal: doctorModal.DoctorCode,
                 excludeId: doctorModal.DoctorId,
                 idCol: "DoctorId");

            if (isDuplicate)
            {
                errors.Add("A doctor data is already exists.");
            }

            if (errors.Any())
            {
                return BadRequest(new { errors });
            }

            try
            {
                var result = _doctorSL.AddUpdatedDoctor(doctorModal);
                string message =doctorModal.DoctorId > 0 ? "Doctor updated successfully." : "Doctor added successfully.";
                return Success(message, result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public IActionResult GetDoctorsByFilter([FromQuery] FilterModel filterModel)
        {

            var doctors = _doctorSL.GetDoctorByFilter(filterModel);

            return Ok(new { data = doctors });
        }

        [HttpGet]
        public IActionResult GetDoctorById(int DoctorId)
        {

            var doctor = _doctorSL.GetDoctorById(DoctorId);
            return Ok(doctor);

        }


        [HttpDelete]
        public IActionResult DeleteDoctorById(int DoctorId)
        {

            try
            {

                if (DoctorId <= 0)
                {
                    return BadRequest("Invalid branch ID.");
                }


                var result = _doctorSL.DeleteDoctorById(DoctorId);

                if (result == null)
                {
                    return NotFound("Doctor Not Found");
                }

                string status = result.IsActive ? "activated" : "deactivated";
                return Success(status, result);
            }

            catch (Exception ex)
            {
                return Error("Error while Doctor status: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }


        [HttpGet]
        public IActionResult GetDoctorIsActive()
        {
            var doctors = _doctorSL.GetDoctorIsActive();

            if (doctors == null)
            {
                return NotFound(new { message = $"Doctor not found." });
            }

            return Ok(doctors);
        }
    }
}
