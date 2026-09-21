#!/usr/bin/env bash
set -euo pipefail

# Standalone containerized FunctionGemma model execution
IMAGE_NAME="${MODEL_IMAGE:-financial-statistics/functiongemma-model:latest}"
CONTAINER_NAME="functiongemma-model"
PORT="${MODEL_PORT:-8080}"
MODELS_DIR="${PWD}/models"

echo "Starting containerized FunctionGemma model service..."

# Stop and remove existing container if running
if docker ps -a --format '{{.Names}}' | grep -Eq "^${CONTAINER_NAME}\$"; then
    echo "Stopping existing container: ${CONTAINER_NAME}..."
    docker rm -f "${CONTAINER_NAME}" >/dev/null 2>&1 || true
fi

docker run -d \
  --name "${CONTAINER_NAME}" \
  -p "${PORT}:8080" \
  -v "${MODELS_DIR}:/models" \
  --restart unless-stopped \
  "${IMAGE_NAME}"

echo "FunctionGemma model container started on http://localhost:${PORT}"
