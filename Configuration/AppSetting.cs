using Microsoft.Extensions.Configuration;

namespace DynamicEndpoint.Configuration
{
    public record AppSetting(IConfiguration _configuration)
    {
        public string ReadConnectionStr => _configuration.GetValue<string>("ConnectionStrings:ReadConnectionStr") ?? throw new ArgumentNullException();

        public string WriteConnectionStr => _configuration.GetValue<string>("ConnectionStrings:WriteConnectionStr") ?? throw new ArgumentNullException();
    }
}
