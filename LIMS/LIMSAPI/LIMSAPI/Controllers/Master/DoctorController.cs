﻿using System.Net;
using LIMSAPI.Models;
using LIMSAPI.Models.Master;
using LIMSAPI.RepositryLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LIMSAPI.Controllers.MasterController
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
                idCol: "DoctorId",
                additionalConditions : new Dictionary<string, object>()
            );

            if (isDuplicate)
            {
                errors.Add("A doctor with this data already exists.");
            }

            var isPhoneDuplicate = _doctorSL.IsDuplicate(
                    table: "doctor",
                    nameCol: "PhoneNumber",
                   codeCol: "PhoneNumber", 
                   nameVal: doctorModal.PhoneNumber,
                   codeVal: doctorModal.PhoneNumber,
                  excludeId: doctorModal.DoctorId,
                 idCol: "DoctorId"
                );

            if (isPhoneDuplicate)

            {
                errors.Add("This phone number is already registered with another doctor.");
            }

            if (errors.Any())
            {
                return BadRequest(new { errors });
            }

            try
            {
                var result = _doctorSL.AddUpdatedDoctor(doctorModal);
                string message = doctorModal.DoctorId > 0 ? "Doctor updated successfully." : "Doctor added successfully.";
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
                    return BadRequest("Invalid doctor ID.");
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
