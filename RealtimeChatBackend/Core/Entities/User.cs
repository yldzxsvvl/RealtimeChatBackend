using System;
using System.Collections.Generic; // List için

namespace Core.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // YENİ EKLENDİ

        // Kullanıcının katıldığı grupların ID'leri (veritabanında string olarak saklanacak)
        public List<Guid> JoinedGroupIds { get; set; } = new List<Guid>(); // YENİ EKLENDİ

        // Navigation property: Kullanıcının gönderdiği mesajlar
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    }
}