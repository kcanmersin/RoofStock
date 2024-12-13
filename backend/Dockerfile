FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY ["API/API.csproj", "API/"]
COPY ["Core/Core.csproj", "Core/"]
RUN dotnet restore "API/API.csproj"

COPY . . 
WORKDIR /src/API
RUN dotnet build "API.csproj" -c Release -o /app/build
RUN dotnet publish "API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build-env /app/publish .

# Swagger için 5244 portunu expose et
EXPOSE 5244

# ASP.NET Core'ı 5244 portunda başlat
ENV ASPNETCORE_URLS=http://0.0.0.0:5244

ENTRYPOINT ["dotnet", "API.dll"]
