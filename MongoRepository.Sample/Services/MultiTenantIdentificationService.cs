namespace MongoRepository.Sample.Services
{
    public interface IMultiTenantIdentificationService : ITenantIdentificationService;

    public class MultiTenantIdentificationService : IMultiTenantIdentificationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor; // just example

        public MultiTenantIdentificationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentTenantSuffix()
        {
            // return your own suffix base your auth.
            if (_httpContextAccessor.HttpContext != null
                && _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("x-tenant-key", out var tenantKey))
            {
                // up to you how to match your auth
                var suffix = $"-{tenantKey}";

                return suffix;
            }
            return "-suffix";
        }

        public string GetCurrentTenantPrefix()
        {
            // return your own Prefix base your auth.
            return "prefix-";
        }
    }
}
