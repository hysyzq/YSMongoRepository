
namespace MongoRepository
{
    /// <summary>
    /// Implement this service in your client
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Returns the Audit object
        /// </summary>
        /// <typeparam name="TAudit"></typeparam>
        /// <param name="auditOperation"></param>
        /// <returns></returns>
        TAudit GetAuditInformation<TAudit>(string auditOperation) where TAudit : IAudit, new();
    }
}
