using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICacheService
    {
        // Bir değeri önbellekten getirir
        Task<string?> GetCacheValueAsync(string key);

        // Bir değeri önbelleğe kaydeder (belirli bir süre için)
        Task SetCacheValueAsync(string key, string value, TimeSpan duration);

        // Bir değeri önbellekten siler
        Task RemoveCacheValueAsync(string key);
    }
}
