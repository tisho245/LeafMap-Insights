#!/bin/bash
# Start Auth API (5203) and Data API (5202). Run from repo root: bash start-apis.sh
# Press Enter to stop both.
ROOT="$(cd "$(dirname "$0")" && pwd)"
AUTH_DIR="$ROOT/LeafMapInsightsAuthAPI/LeafMapInsightsAuthAPI"
DATA_DIR="$ROOT/ASPLeadMapInsightsAPI/ASPLeadMapInsightsAPI"

if [ ! -f "$AUTH_DIR/LeafMapInsightsAuthAPI.csproj" ]; then
  echo "Error: Auth API not found in $AUTH_DIR"
  exit 1
fi
if [ ! -f "$DATA_DIR/ASPLeadMapInsightsAPI.csproj" ]; then
  DATA_DIR="$ROOT/ASPLeadMapInsightsAPI"
  if [ ! -f "$DATA_DIR/ASPLeadMapInsightsAPI.csproj" ]; then
    echo "Error: Data API not found in $ROOT/ASPLeadMapInsightsAPI"
    exit 1
  fi
fi

# 1. Kill processes on 5202 and 5203 so ports are free
for port in 5202 5203; do
  if command -v fuser &>/dev/null; then
    fuser -k "${port}/tcp" 2>/dev/null
  else
    pid=$(lsof -t -i:"${port}" 2>/dev/null)
    [ -n "$pid" ] && kill $pid 2>/dev/null
  fi
done
sleep 2

# 2. Remove obj/bin to avoid permission denied on next build
rm -rf "$AUTH_DIR/obj" "$AUTH_DIR/bin" "$DATA_DIR/obj" "$DATA_DIR/bin" 2>/dev/null
sleep 1

# 3. Start Auth API then Data API (Auth must bind first)
cd "$AUTH_DIR" || exit 1
ASPNETCORE_URLS="http://0.0.0.0:5203" dotnet run &
AUTH_PID=$!
sleep 5
cd "$DATA_DIR" || exit 1
ASPNETCORE_URLS="http://0.0.0.0:5202" dotnet run &
DATA_PID=$!

echo "Auth API PID: $AUTH_PID (port 5203), Data API PID: $DATA_PID (port 5202)"
echo "Press Enter to stop both."
read
kill $AUTH_PID $DATA_PID 2>/dev/null
echo "Stopped."
