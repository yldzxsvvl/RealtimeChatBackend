
# Real-Time Chat Backend Servisi

Bu proje, **Bluesense Backend Engineer Assignment** görevi kapsamında geliştirilmiş, ASP.NET Core tabanlı, ölçeklenebilir ve gerçek zamanlı bir sohbet uygulaması için backend servisidir.

Proje, modern .NET teknolojileri kullanılarak, temiz kod ve sağlam mimari prensipleriyle geliştirilmiştir. Kimlik doğrulama, grup yönetimi, gerçek zamanlı mesajlaşma ve dosya yükleme gibi temel özellikleri içerir. Ayrıca Docker ile konteynerleştirilmiş ve GitHub Actions ile CI/CD otomasyonu sağlanmıştır.

## Ana Özellikler (Key Features)

- **Auth (Kimlik Doğrulama):** JWT tabanlı güvenli kullanıcı kaydı (/register) ve girişi (/login).
- **Groups (Gruplar):** Herkese açık/özel gruplar oluşturma, gruplara katılma ve ayrılma.
- **Messages (Mesajlar):** Grup içinde metin ve dosya (resim, belge vb.) gönderimi.
- **Real-time (Gerçek Zamanlı):** SignalR kullanılarak WebSocket üzerinden anlık mesaj güncellemeleri.
- **Listing & Search (Listeleme ve Arama):** Grup içi mesajlarda ve herkese açık gruplarda arama yapma.

## Teknoloji Yığını (Tech Stack)

- **Framework:** ASP.NET Core 8 (Web API)
- **Veritabanı:** Entity Framework Core & SQLite
- **Gerçek Zamanlı İletişim:** SignalR
- **Test:** xUnit
- **Konteynerleştirme:** Docker & Docker Compose
- **CI/CD:** GitHub Actions

## Yerel Kurulum ve Çalıştırma (Local Setup)

```bash
# Depoyu Kopyalayın
git clone https://github.com/yldzxsvvl/RealtimeChatBackend.git

# Proje Dizinine Gidin
cd YeniChatProjesi

# Gerekli Paketleri Yükleyin
dotnet restore

# Uygulamayı Başlatın
dotnet run --project RealtimeChatBackend
```

Uygulama artık `http://localhost:5222` adresinde çalışıyor olacaktır.

## Dağıtım (Deployment) Adımları

### Docker ile Dağıtım (Tavsiye Edilen Yöntem)

1. **Sunucu Hazırlığı:** Docker ve Docker Compose kurulu bir sunucu hazırlayın.
2. **Kodu Sunucuya Çekin:** `git clone` ile projeyi sunucuya alın.
3. **Üretim Yapılandırması:** `appsettings.Production.json` dosyasında ortam değişkenlerini (DB bağlantısı, JWT anahtarı vb.) tanımlayın.
4. **Uygulamayı Başlatın:**

```bash
docker-compose up -d --build
```

5. **Reverse Proxy ve SSL:** Nginx veya Caddy ile reverse proxy kurun, Let's Encrypt ile SSL aktif edin.

## Testler

```bash
dotnet test
```

## API Kullanımı ve Dokümantasyonu

- Swagger UI: [http://localhost:5222/swagger](http://localhost:5222/swagger)
- SignalR Test İstemcisi: [http://localhost:5222/SignalRTestClient.html](http://localhost:5222/SignalRTestClient.html)

## Mimari ve Teknik Kararlar

### Onion Architecture

```
.
├── Core/                # İş mantığı ve varlıklar
├── Application/         # Arayüzler ve servis kontratları
├── Infrastructure/      # Veritabanı ve dış servisler
└── API/                 # Controller, Hub ve dış dünya ile etkileşim
```

### Trade-offs

- **Veritabanı:** PostgreSQL yerine SQLite tercih edilmiştir. PostgreSQL’e geçiş kolaydır.
- **Önbellekleme:** Redis yerine InMemoryCacheService kullanılmıştır. Redis’e geçiş hazırdır.

## CI/CD Pipeline

GitHub Actions ile:

1. Kod indirilir.
2. .NET 8 SDK kurulur.
3. `dotnet restore` ile bağımlılıklar yüklenir.
4. `dotnet build` ile derlenir.
5. `dotnet test` ile testler çalıştırılır.
