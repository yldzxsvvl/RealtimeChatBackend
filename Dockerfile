# Aşama 1: Uygulamayı derlemek için SDK imajını kullan
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Çalışma dizinini ayarla
WORKDIR /src

# Proje dosyalarını kopyala
# .csproj dosyasını kopyalayarak bağımlılıkların önbelleğe alınmasını sağlarız
COPY ["RealtimeChatBackend.csproj", "RealtimeChatBackend/"]

# Bağımlılıkları geri yükle
# Bu adım, NuGet paketlerini indirir ve önbelleğe alır.
RUN dotnet restore "RealtimeChatBackend/RealtimeChatBackend.csproj"

# Tüm proje dosyalarını kopyala
COPY . .

# Çalışma dizinini proje klasörüne değiştir
WORKDIR "/src/RealtimeChatBackend"

# Uygulamayı yayımla (Release modunda derle)
# Bu, uygulamanın çalıştırılabilir bir sürümünü oluşturur.
RUN dotnet publish "RealtimeChatBackend.csproj" -c Release -o /app/publish \
    --no-restore # Bağımlılıklar zaten geri yüklendi


# Aşama 2: Uygulamayı çalıştırmak için ASP.NET Runtime imajını kullan
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Çalışma dizinini ayarla
WORKDIR /app

# wwwroot/Uploads klasörünü oluştur (statik dosyalar ve yüklemeler için)
# Bu klasörün varlığını garanti ederiz.
RUN mkdir -p wwwroot/Uploads

# Bir önceki aşamadan yayımlanmış uygulama dosyalarını kopyala
COPY --from=build /app/publish .

# Uygulama başladığında çalıştırılacak komutu belirle
# Bu, uygulamanın ana DLL dosyasını çalıştırır.
ENTRYPOINT ["dotnet", "RealtimeChatBackend.dll"]
