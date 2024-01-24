using Microsoft.AspNetCore.Http;
using Pulse.Account.Core.Interfaces;

namespace Account.Api.Tests.Configurations
{
    public class AuthenticationContextMOCK : IAuthenticationContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationContextMOCK(IHttpContextAccessor? httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string BearerToken
        {
            get
            {
                return "test";
            }
        }
    }
}
