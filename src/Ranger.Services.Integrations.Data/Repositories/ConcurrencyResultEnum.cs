namespace Ranger.Services.Integrations.Data
{
    public enum ConcurrencyResultEnum
    {
        NO_CHANGE = 0,
        VERSION_OUTDATED = 1,
        VERSION_TOO_HIGH = 2,
        SUCCESS = 3
    }
}