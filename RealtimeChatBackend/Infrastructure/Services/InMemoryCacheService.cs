using Application.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class InMemoryCacheService : ICacheService
    {
        private readonly ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();

       
        public Task<string?> GetCacheValueAsync(string key)
        {
            _cache.TryGetValue(key, out var value);
            return Task.FromResult(value);
        }

       
        public Task SetCacheValueAsync(string key, string value, TimeSpan expiry)
        {
            _cache[key] = value;
         
            return Task.CompletedTask;
        }

        public Task RemoveCacheValueAsync(string key)
        {
            _cache.TryRemove(key, out _);
            return Task.CompletedTask;
        }
    }
}
