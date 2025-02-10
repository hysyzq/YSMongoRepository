
using System.IdentityModel.Tokens.Jwt;

namespace MongoRepository.Sample.Services
{

    public class AuditService : IAuditService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;


        /// <summary>
        /// This avoid decode multiple time. 
        /// </summary>
        private string? _auditUser = "";

        public AuditService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }


        public TAudit GetAuditInformation<TAudit>(string auditOperation) where TAudit : IAudit, new()
        {
            if (string.IsNullOrWhiteSpace(_auditUser))
            {
                var authorization = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                if (authorization is { Length: > 7 })
                {
                    var token = authorization[7..];
                    var handler = new JwtSecurityTokenHandler();
                    var decoded = handler.ReadJwtToken(token);
                    var email = decoded.Claims.FirstOrDefault(t => string.Equals(t.Type, "email", StringComparison.CurrentCultureIgnoreCase))?.Value;
                    _auditUser = email;
                }
            }
            var audit = new TAudit { Operation = auditOperation, OperatedBy = _auditUser??"default@email.com" };

            return audit;
        }
    }
}
