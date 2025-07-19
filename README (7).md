
# Real-Time Chat Backend Servisi

Bu proje, **Bluesense Backend Engineer Assignment** görevi kapsamında geliştirilmiş, ASP.NET Core tabanlı, ölçeklenebilir ve gerçek zamanlı bir sohbet uygulaması için backend servisidir.

Proje, modern .NET teknolojileri kullanılarak, temiz kod ve sağlam mimari prensipleriyle geliştirilmiştir. Kimlik doğrulama, grup yönetimi, gerçek zamanlı mesajlaşma ve dosya yükleme gibi temel özellikleri içerir. Ayrıca Docker ile konteynerleştirilmiş ve GitHub Actions ile CI/CD otomasyonu sağlanmıştır.

## 🔑 Ana Özellikler (Key Features)

- **Auth (Kimlik Doğrulama):** JWT tabanlı güvenli kullanıcı kaydı (`/register`) ve girişi (`/login`).
- **Groups (Gruplar):** Herkese açık/özel gruplar oluşturma, gruplara katılma ve ayrılma.
- **Messages (Mesajlar):** Grup içinde metin ve dosya (resim, belge vb.) gönderimi.
- **Real-time (Gerçek Zamanlı):** SignalR kullanılarak WebSocket üzerinden anlık mesaj güncellemeleri.
- **Listing & Search (Listeleme ve Arama):** Grup içi mesajlarda ve herkese açık gruplarda arama yapma.

## 🛠 Teknoloji Yığını (Tech Stack)

- **Framework:** ASP.NET Core 8 (Web API)
- **Veritabanı:** Entity Framework Core & SQLite
- **Gerçek Zamanlı İletişim:** SignalR
- **Test:** xUnit
- **Konteynerleştirme:** Docker & Docker Compose
- **CI/CD:** GitHub Actions

## 💻 Yerel Kurulum ve Çalıştırma (Local Setup)

Projeyi yerel makinenizde çalıştırmak için aşağıdaki adımları izleyin:

```bash
# Depoyu Klonlayın
git clone https://github.com/yldzxsvvl/RealtimeChatBackend.git

# Proje Dizinine Gidin
cd YeniChatProjesi
# Not: Eğer klonladığınız klasörün adı farklıysa, o klasörün içine girin.

# Gerekli Paketleri Yükleyin
dotnet restore

# Uygulamayı Başlatın
dotnet run --project RealtimeChatBackend
```

Uygulama artık `http://localhost:5222` adresinde çalışıyor olacaktır.

## 🐳 Docker ile Çalıştırma

Docker konteynerleri içinde çalıştırmak için:

```bash
docker-compose up --build
```

> Not: Docker'ın kurulu ve çalışır durumda olması gerekmektedir.

## 🧪 Testler

Birim ve entegrasyon testlerini çalıştırmak için:

```bash
dotnet test
```

## 📘 API Kullanımı ve Dokümantasyonu

Swagger UI: [http://localhost:5222/swagger](http://localhost:5222/swagger)

SignalR Test İstemcisi: [http://localhost:5222/SignalRTestClient.html](http://localhost:5222/SignalRTestClient.html)

## 🧱 Mimari ve Teknik Kararlar

### Mimari Yapı: Onion Architecture

```
.
├── Core/                # İş mantığı ve varlıklar (Entities)
├── Application/         # Arayüzler ve servis kontratları
├── Infrastructure/      # Veritabanı ve harici servis implementasyonları
└── API/                 # Controller'lar, Hub'lar, dış dünyaya açılan kapı
```

### Teknik Tercihler ve Takaslar

- **Veritabanı Seçimi:** PostgreSQL yerine SQLite tercih edilmiştir. EF Core sayesinde PostgreSQL'e geçiş kolaydır.
- **Önbellekleme:** Redis yerine InMemoryCacheService kullanılmıştır. Redis'e geçiş tek satır kodla mümkündür.

## 🔁 CI/CD Pipeline

GitHub Actions ile CI hattı aşağıdaki adımları içerir:

1. Kod indirilir.
2. .NET 8 SDK'sı kurulur.
3. Bağımlılıklar yüklenir (`dotnet restore`).
4. Proje derlenir (`dotnet build`).
5. Tüm testler çalıştırılır (`dotnet test`).
