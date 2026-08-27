#!/usr/bin/env bash
set -e

echo "Dropping database..."
dotnet ef database drop -f

echo "Removing old migrations..."
rm -rf Migrations/

echo "Creating fresh InitialCreate migration..."
dotnet ef migrations add InitialCreate

echo "Applying migration to database..."
dotnet ef database update

echo "Seeding database..."
docker exec -i app_postgres psql -U app -d app <seed.sql

echo "Done. Database reset and seeded."
