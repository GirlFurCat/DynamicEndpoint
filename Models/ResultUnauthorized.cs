namespace DynamicEndpoint.Models
{
    public record ResultUnauthorized()
    {
        public int status => 401;
        public string error => "Unauthorized";
        public string message => "身份验证失败，缺少或无效的访问令牌。";
    }
}
