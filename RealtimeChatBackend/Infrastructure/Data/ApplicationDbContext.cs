using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; // ValueComparer için gerekli
using System;
using System.Collections.Generic; // List için
using System.Linq;

namespace Infrastructure.Data
{
    /// <summary>
    /// Uygulamanın veritabanı bağlamı (context).
    /// Entity Framework Core'un veritabanıyla iletişim kurmasını sağlar.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Veritabanındaki tablolara karşılık gelen DbSet'ler
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Message> Messages { get; set; }

       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

     
            var guidListComparer = new ValueComparer<List<Guid>>(
                // Eşitlik kontrolü: İki listenin de elemanları aynı sırada ve aynıysa true döner.
                (l1, l2) => (l1 == null && l2 == null) || (l1 != null && l2 != null && l1.SequenceEqual(l2)),
                // Hash kodu oluşturma: Listenin içeriğine göre bir hash kodu üretir.
                l => l.Aggregate(0, (hash, guid) => HashCode.Combine(hash, guid.GetHashCode())),
                // Klonlama (Snapshot): Değişiklik takibi için listenin bir kopyasını oluşturur.
                l => l.ToList()
            );

            // --- User Entity Yapılandırması ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id); // Primary Key
                entity.HasIndex(u => u.Email).IsUnique(); // Email alanı benzersiz olmalı
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();

                // JoinedGroupIds (List<Guid>) alanının veritabanında nasıl saklanacağını belirler.
                entity.Property(u => u.JoinedGroupIds)
                    .HasConversion( // Değer Dönüşümü
                        // C# List<Guid> -> Veritabanı string
                        v => string.Join(',', v),
                        // Veritabanı string -> C# List<Guid>
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList()
                    )
                   
                    .Metadata.SetValueComparer(guidListComparer);
            });

            
            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(g => g.Id); // Primary Key
                entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
                entity.Property(g => g.Description).HasMaxLength(500);

             
                entity.Property(g => g.MemberIds)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList()
                    )
                    .Metadata.SetValueComparer(guidListComparer);

                // İlişki: Bir grubun çok sayıda mesajı olabilir (One-to-Many)
                entity.HasMany(g => g.Messages)
                      .WithOne(m => m.Group)
                      .HasForeignKey(m => m.GroupId)
                      .OnDelete(DeleteBehavior.Cascade); // Bir grup silinirse, o gruba ait tüm mesajlar da silinir.
            });

            // --- Message Entity Yapılandırması ---
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(m => m.Id); // Primary Key
                entity.Property(m => m.Content).HasMaxLength(1000);
                entity.Property(m => m.FileUrl).HasMaxLength(500);

                // İlişki: Bir mesajın bir göndereni (User) vardır (Many-to-One)
                entity.HasOne(m => m.Sender)
                      .WithMany(u => u.SentMessages)
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.Restrict); // Bir kullanıcı silinmeye çalışılırsa ve gönderdiği mesajlar varsa, veritabanı bu silme işlemini engeller.
            });
        }
    }
}