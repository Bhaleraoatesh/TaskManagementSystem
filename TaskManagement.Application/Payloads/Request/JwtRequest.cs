
using System.Text.Json.Serialization;

namespace TaskManagement.Application.Payloads.Request
{
    public class JsonPropertyName
    {
        [JsonPropertyName("emailOrPassword")]
        public string? Username { get; set; } 
        public string? password { get; set; }   
    }
}
