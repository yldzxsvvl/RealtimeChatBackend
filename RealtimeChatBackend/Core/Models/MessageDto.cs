using System;

namespace Core.Models
{
    // İstemciye gönderilecek basitleştirilmiş mesaj objesi
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderUsername { get; set; } = null!; 
        public Guid GroupId { get; set; }
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
