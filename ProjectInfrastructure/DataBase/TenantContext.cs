using System.Security.Claims;

namespace FrontOfficeApplication.ProjectInfrastructure.DataBase
{
    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantContext(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string BranchCode
        {
            get
            {
                var value = _httpContextAccessor
                    .HttpContext?
                    .User?
                    .FindFirst("Branch_code")?
                    .Value;

                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new UnauthorizedAccessException(
                        "Branch code is missing from JWT.");
                }

                return value;
            }
        }

        public string UserCode
        {
            get
            {
                var value = _httpContextAccessor
                    .HttpContext?
                    .User?
                    .FindFirst("UserCode")?
                    .Value;

                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new UnauthorizedAccessException(
                        "UserCode is missing from JWT.");
                }

                return value;
            }
        }

        public string UserName
        {
            get
            {
                var value = _httpContextAccessor
                    .HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.Name)?
                    .Value;

                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new UnauthorizedAccessException(
                        "Username is missing from JWT.");
                }

                return value;
            }
        }
    }
}