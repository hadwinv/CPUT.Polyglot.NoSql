namespace CPUT.Polyglot.NoSql.Interface
{
    public interface ICache
    {
        object GetInMemory(string cacheKey);

        void AddToInMemory(string cacheKey, object cacheObject);

        void AddToInMemoryShortDuration(string cacheKey, object cacheObject);

        void ClearInMemoryWithKey(string cacheKey);

        void ClearAllInMemory();
    }
}
