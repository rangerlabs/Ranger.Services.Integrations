namespace Ranger.Services.Integrations
{
    public static class RedisKeys
    {
        public static string TenantDbPassword(string tenantId) => $"DB_PASSWORD_{tenantId}";
    }
}