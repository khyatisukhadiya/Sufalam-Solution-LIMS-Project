using System.Text.Json.Serialization;

namespace LIMSAPI.Models.Master
{
    public class ServiceRequestDto
    {


        public ServiceModal Service { get; set; }



        public List<TestModal> Tests { get; set; }
    }
}
