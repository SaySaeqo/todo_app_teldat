#!/bin/bash

# Start frontend with hot reload

echo "Starting frontend in development mode..."
echo "Listening on: http://localhost:5173"
echo ""

cd "$(dirname "$0")" || exit 1

npm run dev
