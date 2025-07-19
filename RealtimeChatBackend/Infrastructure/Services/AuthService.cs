// Gerekli kütüphaneler ve isim alanları
using Application.Interfaces; // IAuthService arayüzünün geldiği yer
using Core.Entities;          // User entity'sinin geldiği yer
using Core.Models;            // JwtSettings modelinin geldiği yer
using Infrastructure.Data;    // ApplicationDbContext'in geldiği yer
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    /// <summary>
    /// Kimlik doğrulama işlemlerini (kayıt, giriş) yöneten servis.
    /// IAuthService arayüzünü uygular.
    /// </summary>
    public class AuthService : IAuthService
    {
        // JWT ayarlarını tutan nesne (appsettings.json'dan okunur)
        private readonly JwtSettings _jwtSettings;
        // Veritabanı işlemleri için Entity Framework Core context'i
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor: Gerekli servisleri Dependency Injection ile alır.
        /// </summary>
        /// <param name="jwtSettings">IOptions<JwtSettings> ile JWT ayarlarını alır.</param>
        /// <param name="context">ApplicationDbContext'i alır.</param>
        public AuthService(IOptions<JwtSettings> jwtSettings, ApplicationDbContext context)
        {
            // IOptions<T>.Value ile ayarlara erişilir
            _jwtSettings = jwtSettings.Value;
            _context = context;
        }

        /// <summary>
        /// Sistemdeki tüm kullanıcıları döner.
        /// </summary>
        public List<User> GetUsers()
        {
            return _context.Users.ToList();
        }

        /// <summary>
        /// Yeni bir kullanıcı kaydı yapar.
        /// </summary>
        /// <param name="username">Kullanıcı adı.</param>
        /// <param name="email">Kullanıcının e-posta adresi.</param>
        /// <param name="password">Kullanıcının parolası (hash'lenecektir).</param>
        /// <returns>Kayıt başarılıysa JWT token, e-posta zaten mevcutsa null döner.</returns>
        public async Task<string?> RegisterAsync(string username, string email, string password)
        {
            // 1. E-posta adresinin zaten kullanımda olup olmadığını kontrol et
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
            {
                return null; // E-posta zaten mevcutsa kayıt başarısızdır.
            }

            // 2. Yeni kullanıcı nesnesini oluştur ve parolayı güvenli bir şekilde hash'le
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                // Parolayı asla düz metin olarak saklama! BCrypt ile hash'le.
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAt = DateTime.UtcNow,
          
                JoinedGroupIds = new List<Guid>()
            };

            // 3. Yeni kullanıcıyı veritabanına ekle
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

         
            return GenerateJwtToken(newUser);
        }

        /// <summary>
        /// Kullanıcının sisteme giriş yapmasını sağlar.
        /// </summary>
        /// <param name="email">Kullanıcının e-posta adresi.</param>
        /// <param name="password">Kullanıcının parolası.</param>
        /// <returns>Giriş başarılıysa JWT token, aksi takdirde null döner.</returns>
        public async Task<string?> LoginAsync(string email, string password)
        {
            // 1. E-posta adresine göre kullanıcıyı bul
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            // 2. Kullanıcı bulunamazsa veya parola yanlışsa giriş başarısızdır.
        
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }

            // 3. Giriş başarılı olduğunda JWT token oluştur ve döndür
            return GenerateJwtToken(user);
        }

       

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
          
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var now = DateTime.UtcNow;
          
            var expires = now.AddMinutes(_jwtSettings.ExpiryMinutes);

          
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                  
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Kullanıcı ID'si
                    new Claim(ClaimTypes.Email, user.Email),                   // E-posta
                    new Claim(ClaimTypes.Name, user.Username)                  // Kullanıcı Adı
                }),
                Expires = expires, // Token'ın son geçerlilik tarihi
                NotBefore = now,   // Token'ın hangi tarihten itibaren geçerli olacağı
                // Token'ı imzalamak için kullanılacak anahtar ve şifreleme algoritması
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                // Token'ı kimin oluşturduğu (Issuer) ve kimin için olduğu (Audience) bilgileri
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            // Token'ı oluştur ve string formatına çevir
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}