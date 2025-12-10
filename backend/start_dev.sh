#!/bin/bash

# Start backend with hot reload

echo "Starting backend in development mode..."
echo "Listening on: https://localhost:7030 (http://localhost:5254)"
echo ""

cd "$(dirname "$0")" || exit 1

dotnet watch run --non-interactive --launch-profile https
