#!/usr/bin/env bash
set -e

echo "Starting Postgres and Redis..."
docker compose up -d

echo "Waiting for services to be healthy..."
until [ "$(docker inspect -f '{{.State.Health.Status}}' app_postgres)" == "healthy" ] &&
  [ "$(docker inspect -f '{{.State.Health.Status}}' redis-oppmonitor)" == "healthy" ]; do
  sleep 1
done

echo "Services ready. Starting API..."
dotnet run --project .
