using System;
using System.Collections.Generic;

namespace Core.Entities
{
    public class Message
    {
        public Guid Id { get; set; } // Mesajın benzersiz ID'si
        public Guid SenderId { get; set; } // Mesajı gönderen kullanıcının ID'si
        public User? Sender { get; set; } // Mesajı gönderen kullanıcı (navigasyon özelliği)
        public Guid GroupId { get; set; } // Mesajın gönderildiği grubun ID'si
        public Group? Group { get; set; } // Mesajın gönderildiği grup (navigasyon özelliği)
        public string? Content { get; set; } // Mesajın içeriği (metin)
        public string? FileUrl { get; set; } // Yüklenen dosyanın URL'si (null olabilir)
        public string? FileName { get; set; } // Yüklenen dosyanın adı (null olabilir)

       
        public DateTime Timestamp { get; set; } // Mesajın gönderilme tarihi ve saati
        public DateTime? EditedAt { get; set; } // Mesajın düzenlenme tarihi (null olabilir)
        public bool IsDeleted { get; set; } = false; // Mesajın silinip silinmediği (soft delete için)
    }
}
