using Tenanzia.API.Interfaces;

namespace Tenanzia.API.Services
{
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        /*
         * IHttpContextAccessor
          وده  Service جاهزة من ASP.NET 
           تخليني أوصل للـ HttpContext من أي مكان 
           وال HttpContext ده بيحتوي على معلومات عن الطلب الحالي زي المستخدم والتوكن والـ Claims اللي جايه مع التوكن
        من الاخر يعني اللي فيه ال TenantId اللي احنا ضايفينه في التوكن لما بنولده في الـ AuthService
         */
        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetTenantId()
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst("TenantId")?.Value;

            if (claim == null)
                throw new UnauthorizedAccessException("TenantId not found in token");

            return int.Parse(claim);
        }
    }
}
