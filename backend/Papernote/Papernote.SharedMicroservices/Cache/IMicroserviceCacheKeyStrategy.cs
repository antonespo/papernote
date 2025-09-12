namespace Papernote.SharedMicroservices.Cache;

public interface IMicroserviceCacheKeyStrategy
{
    string ServicePrefix { get; }
    string Version { get; }
}

public interface IAdvancedCacheKeyStrategy : IMicroserviceCacheKeyStrategy
{
    string GetPatternKey(string operation, string wildcard = "*");
    string GetVersionedKey(string operation, params string[] segments);
}