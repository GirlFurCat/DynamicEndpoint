using System.Text.Json.Serialization;

namespace DynamicEndpoint.Models
{
    public record ResultBadRequest([property: JsonIgnore] List<string> para)
    {
        public int status => 400;
        public string error => "Bad Request";
        public string message => "Missing required fields.";
        public dynamic details => para.Select(x => new
        {
            field = x,
            message = $"The {x} field is required."
        }).ToList();
    }
}
