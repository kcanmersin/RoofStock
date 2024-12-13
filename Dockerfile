# Stage 1: Build AI Service
FROM python:3.9-slim AS ai-build
WORKDIR /ai
COPY AI/requirements.txt ./requirements.txt
RUN pip install --no-cache-dir -r requirements.txt
COPY AI /ai

# Stage 2: Build Backend Service
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src
COPY backend/API/API.csproj API/
COPY backend/Core/Core.csproj Core/
RUN dotnet restore API/API.csproj
COPY backend /src
WORKDIR /src/API
RUN dotnet build API.csproj -c Release -o /app/build
RUN dotnet publish API.csproj -c Release -o /app/publish

# Stage 3: Build Frontend Service
FROM node:16 AS frontend-build
WORKDIR /frontend
COPY frontend/package.json frontend/package-lock.json ./
RUN npm install
COPY frontend /frontend
RUN npm run build

# Stage 4: Combine Everything
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy Backend
COPY --from=backend-build /app/publish ./backend

# Copy Frontend
COPY --from=frontend-build /frontend/build ./wwwroot

# Copy AI Service
COPY --from=ai-build /ai ./ai

# Expose ports
EXPOSE 5244 5000 3000

# Set environment variables
ENV ASPNETCORE_URLS=http://0.0.0.0:5244

# Command to start backend and AI service using supervisord
RUN apt-get update && apt-get install -y supervisor
COPY supervisord.conf /etc/supervisor/conf.d/supervisord.conf
CMD ["supervisord", "-n"]
