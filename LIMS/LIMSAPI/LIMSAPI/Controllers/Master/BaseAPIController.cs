using System.Net;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LIMSAPI.Controllers.MasterController
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseAPIController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BaseAPIController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, object content)
        {
            var json = JsonSerializer.Serialize(content);
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }

        public class BaseResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }

            public BaseResponse(bool success, string message, T data = default)
            {
                Success = success;
                Message = message;
                Data = data;
            }
        }

        protected IActionResult Success<T>(string message, T data)
        {
            var response = new BaseResponse<T>(true, message, data);
            return Ok(response);
        }

        protected IActionResult Error(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            var response = new BaseResponse<object>(false, message);
            return StatusCode((int)statusCode, response);
        }
    }
}
