namespace MongoRepository
{
    public interface ITenantIdentificationService
    {
        string GetCurrentTenantSuffix();

        string GetCurrentTenantPrefix();
    }
}
