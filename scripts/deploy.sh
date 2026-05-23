#!/usr/bin/env bash
# Deploy the Unitask API on an EC2 host.
# Run this from the repo root on the EC2 instance:
#   bash scripts/deploy.sh
#
# Requires: docker, docker compose, a populated .env in the repo root.

set -euo pipefail

cd "$(dirname "$0")/.."

if [[ ! -f .env ]]; then
  echo "[deploy] .env not found. Copy .env.example to .env and fill it in."
  exit 1
fi

echo "[deploy] Pulling latest source..."
git pull --ff-only

echo "[deploy] Building image..."
docker compose build

echo "[deploy] Starting container..."
docker compose up -d

echo "[deploy] Pruning dangling images..."
docker image prune -f

echo "[deploy] Waiting for /health to respond..."
for i in {1..30}; do
  if curl -fsS http://127.0.0.1/health >/dev/null 2>&1; then
    echo "[deploy] OK - service healthy."
    docker compose ps
    exit 0
  fi
  sleep 2
done

echo "[deploy] FAIL - service did not become healthy in 60s."
docker compose logs --tail=80 api
exit 1
