#!/bin/bash

# PostgreSQL'in hazır olmasını bekle
/app/wait-for-it.sh db:5432 --timeout=60 --strict -- echo "PostgreSQL is up"

# Veritabanı migrasyonlarını çalıştır
dotnet ef database update --project /src/Core --startup-project /src/API

# Uygulamayı başlat
dotnet API.dll
