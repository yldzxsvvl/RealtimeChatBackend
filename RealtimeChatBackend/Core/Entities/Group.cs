using System;
using System.Collections.Generic;

namespace Core.Entities
{
    public class Group
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; } // Açıklama opsiyonel olabilir
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // YENİ EKLENDİ
        public bool IsPublic { get; set; } // Grubun herkese açık olup olmadığı
        public Guid CreatorId { get; set; } // Grubu oluşturan kullanıcının ID'si

        // Gruba üye olan kullanıcıların ID'leri (veritabanında string olarak saklanacak)
        public List<Guid> MemberIds { get; set; } = new List<Guid>(); 

        // Navigation property: Gruba ait mesajlar
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
