#!/bin/bash
# Стартира и трите: Auth API (5203), Data API (5202), Node.js клиент (3000).
# Изпълнявай от корена на репото: bash start-all.sh
# Натисни Enter за спиране на всички.

ROOT="$(cd "$(dirname "$0")" && pwd)"
AUTH_DIR="$ROOT/LeafMapInsightsAuthAPI/LeafMapInsightsAuthAPI"
DATA_DIR="$ROOT/ASPLeadMapInsightsAPI/ASPLeadMapInsightsAPI"
NODE_DIR="$ROOT/LeafMapInsightsNodeClient/LeafMapInsightsNodeClient"

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
if [ ! -f "$NODE_DIR/package.json" ] || [ ! -f "$NODE_DIR/server.js" ]; then
  echo "Error: Node client not found in $NODE_DIR"
  exit 1
fi

# 1. Освобождаване на портове 5202, 5203, 3000
for port in 5202 5203 3000; do
  if command -v fuser &>/dev/null; then
    fuser -k "${port}/tcp" 2>/dev/null
  else
    pid=$(lsof -t -i:"${port}" 2>/dev/null)
    [ -n "$pid" ] && kill $pid 2>/dev/null
  fi
done
sleep 2

# 2. Премахване на obj/bin за по-чист build на API-тата
rm -rf "$AUTH_DIR/obj" "$AUTH_DIR/bin" "$DATA_DIR/obj" "$DATA_DIR/bin" 2>/dev/null
sleep 1

# 3. Стартиране на Auth API
cd "$AUTH_DIR" || exit 1
ASPNETCORE_URLS="http://0.0.0.0:5203" dotnet run &
AUTH_PID=$!
sleep 5

# 4. Стартиране на Data API
cd "$DATA_DIR" || exit 1
ASPNETCORE_URLS="http://0.0.0.0:5202" dotnet run &
DATA_PID=$!
sleep 5

# 5. Стартиране на Node.js клиент (чете API_BASE_URL и AUTH_API_BASE_URL от .env в NODE_DIR)
cd "$NODE_DIR" || exit 1
# Не задаваме API_BASE_URL/AUTH_API_BASE_URL тук – да се ползват стойностите от .env
node server.js &
NODE_PID=$!

echo ""
echo "Auth API   PID: $AUTH_PID (port 5203)"
echo "Data API   PID: $DATA_PID (port 5202)"
echo "Node client PID: $NODE_PID (port 3000)"
echo "Уеб интерфейс: http://localhost:3000"
echo ""
echo "Натисни Enter за спиране на всички."
read
kill $AUTH_PID $DATA_PID $NODE_PID 2>/dev/null
echo "Спиране на всички процеси."
