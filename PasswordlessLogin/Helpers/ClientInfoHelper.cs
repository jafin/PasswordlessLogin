using Microsoft.AspNetCore.Http;

namespace SimpleIAM.PasswordlessLogin.Helpers
{
    public interface IClientInfoHelper
    {
        ClientInfoModel GetClientInfo();
    }

    public class ClientInfoHelper : IClientInfoHelper
    {
        private readonly HttpContext _httpContext;

        public ClientInfoHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }

        public ClientInfoModel GetClientInfo()
        {
            var clientIp = _httpContext.Connection.RemoteIpAddress?.ToString();
            return new ClientInfoModel
            {
                IpAddress = clientIp
            };
        }
    }
}