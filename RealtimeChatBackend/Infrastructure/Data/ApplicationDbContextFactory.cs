using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Infrastructure.Data
{
    // Bu sınıf, dotnet ef komutlarının (migrations add, database update gibi)
    // tasarım zamanında ApplicationDbContext'i nasıl oluşturacağını belirtir.
    // Bu, Program.cs'teki karmaşık DI yapılandırmasından bağımsız olarak
    // DbContext'in doğru bir şekilde başlatılmasını sağlar.
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // appsettings.json dosyasını okumak için bir ConfigurationBuilder oluştur
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Projenin kök dizinini ayarla
                .AddJsonFile("appsettings.json") // appsettings.json dosyasını ekle
                .Build();

           
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

          
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("DefaultConnection connection string not found in appsettings.json.");
            }

            
            builder.UseSqlite(connectionString);

            return new ApplicationDbContext(builder.Options);
        }
    }
}
