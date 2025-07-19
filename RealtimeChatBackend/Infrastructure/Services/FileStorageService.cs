using Application.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Http; 
using Microsoft.Extensions.Options; 
using System;
using System.IO; 
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _uploadsFolder;
        private readonly string _webRootPath; // wwwroot klasörünün yolu

        // Constructor'a IOptions<FileStorageSettings> ve IWebHostEnvironment'ı enjekte et
        public FileStorageService(IOptions<FileStorageSettings> settings, Microsoft.AspNetCore.Hosting.IWebHostEnvironment webHostEnvironment)
        {
            // appsettings.json'dan UploadsFolder değerini al
            _uploadsFolder = settings.Value.UploadsFolder;
            // wwwroot klasörünün fiziksel yolunu al
            _webRootPath = webHostEnvironment.WebRootPath;
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Dosya boş veya geçersiz.");
            }

          
            var uploadPath = Path.Combine(_webRootPath, _uploadsFolder);

           
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

           
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

         
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            
            return $"/{_uploadsFolder}/{fileName}";
        }

        public void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

         
            var fileName = Path.GetFileName(filePath);
            var fullPath = Path.Combine(_webRootPath, _uploadsFolder, fileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
