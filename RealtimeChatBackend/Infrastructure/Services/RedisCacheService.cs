using Application.Interfaces;
using Core.Models;
using Microsoft.Extensions.Options;
using StackExchange.Redis; // Redis için gerekli
using System;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisCacheService(IOptions<RedisSettings> redisSettings)
        {
            // Redis bağlantısını oluştur
            _redis = ConnectionMultiplexer.Connect(redisSettings.Value.ConnectionString);
            _database = _redis.GetDatabase(); // Varsayılan veritabanını al
        }

        public async Task<string?> GetCacheValueAsync(string key)
        {
            return await _database.StringGetAsync(key);
        }

        public async Task SetCacheValueAsync(string key, string value, TimeSpan duration)
        {
            await _database.StringSetAsync(key, value, duration);
        }

        public async Task RemoveCacheValueAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}
