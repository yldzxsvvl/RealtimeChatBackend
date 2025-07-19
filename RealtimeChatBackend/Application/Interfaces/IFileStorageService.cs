using Microsoft.AspNetCore.Http; // IFormFile için
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFileStorageService
    {
        // Dosyayı kaydeder ve erişilebilecek URL'yi döndürür.
        Task<string> SaveFileAsync(IFormFile file);

        // Belirtilen dosyayı siler.
        void DeleteFile(string filePath);
    }
}
