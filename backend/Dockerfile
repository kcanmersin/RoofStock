# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

# Proje dosyalarını kopyala ve restore yap
COPY ["API/API.csproj", "API/"]
COPY ["Core/Core.csproj", "Core/"]
RUN dotnet restore "API/API.csproj"

# Tüm kaynak kodlarını kopyala ve yayınlama için derle
COPY . .
WORKDIR /src/API
RUN dotnet build "API.csproj" -c Release -o /app/build
RUN dotnet publish "API.csproj" -c Release -o /app/publish

# Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build-env /app/publish .

# Uygulama portunu tanımla
EXPOSE 80

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "API.dll"]
