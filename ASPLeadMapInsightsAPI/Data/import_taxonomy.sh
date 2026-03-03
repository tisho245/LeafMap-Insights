#!/usr/bin/env bash
# Импорт на таксономични данни от taxonomic_import.json в LeafMap Insights API.
# Нужни: curl, jq. Логин с потребител с роля Admin.
#
# Употреба:
#   ./import_taxonomy.sh [път_към_json]
#   AUTH_API_URL=http://localhost:5203 API_BASE_URL=http://localhost:5202 ./import_taxonomy.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
JSON_FILE="${1:-$SCRIPT_DIR/taxonomic_import.json}"
AUTH_API_URL="${AUTH_API_URL:-http://localhost:5203}"
API_BASE_URL="${API_BASE_URL:-http://localhost:5202}"

if [[ -z "$ADMIN_USER" || -z "$ADMIN_PASS" ]]; then
  echo "Задайте ADMIN_USER и ADMIN_PASS (потребител с роля Admin)."
  echo "Пример: ADMIN_USER=admin ADMIN_PASS=secret ./import_taxonomy.sh"
  exit 1
fi

if [[ ! -f "$JSON_FILE" ]]; then
  echo "Файлът не е намерен: $JSON_FILE"
  exit 1
fi

echo "Auth API: $AUTH_API_URL"
echo "Data API: $API_BASE_URL"
echo "JSON:     $JSON_FILE"

# Логин и вземане на JWT
LOGIN_RESP=$(curl -s -X POST "$AUTH_API_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"UserName\":\"$ADMIN_USER\",\"Password\":\"$ADMIN_PASS\"}")

TOKEN=$(echo "$LOGIN_RESP" | jq -r '.token')
if [[ -z "$TOKEN" || "$TOKEN" == "null" ]]; then
  echo "Грешка при логин. Отговор: $LOGIN_RESP"
  exit 1
fi

echo "Логин успешен."

# Добавя запис само ако още няма такова име. Връща 0 ако е добавен, 1 ако вече съществува, 2 при грешка.
add_if_missing() {
  local method="$1"   # divisions, taxonomyclasses, families, genera, species
  local name="$2"
  local existing_list
  existing_list=$(curl -s -H "Authorization: Bearer $TOKEN" "$API_BASE_URL/api/$method")
  if echo "$existing_list" | jq -e --arg n "$name" '[.[] | select(.Name == $n)] | length > 0' >/dev/null 2>&1; then
    return 1
  fi
  local body
  body=$(jq -n --arg n "$name" '{Name: $n}')
  local res
  res=$(curl -s -w "\n%{http_code}" -X POST "$API_BASE_URL/api/$method" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "$body")
  local code
  code=$(echo "$res" | tail -n1)
  if [[ "$code" != "201" && "$code" != "200" ]]; then
    echo "  Грешка при добавяне на $method '$name': HTTP $code"
    return 2
  fi
  echo "  + $name"
  return 0
}

# Извличаме уникални стойности (по един ред на ред за коректна обработка на интервали в имената)
echo "Извличане на данни от JSON..."
mapfile -t divisions < <(jq -r '[.species[].division] | unique | .[]' "$JSON_FILE")
mapfile -t classes < <(jq -r '[.species[].taxonomyClass] | unique | .[]' "$JSON_FILE")
mapfile -t families < <(jq -r '[.species[].family] | unique | .[]' "$JSON_FILE")
mapfile -t genera < <(jq -r '[.species[].genus] | unique | .[]' "$JSON_FILE")
mapfile -t species < <(jq -r '[.species[].species] | unique | .[]' "$JSON_FILE")

added_div=0
added_class=0
added_fam=0
added_gen=0
added_spec=0

echo "Добавяне на отдели (divisions)..."
for n in "${divisions[@]}"; do
  add_if_missing "divisions" "$n" && ((added_div++)) || true
done

echo "Добавяне на класове (taxonomyclasses)..."
for n in "${classes[@]}"; do
  add_if_missing "taxonomyclasses" "$n" && ((added_class++)) || true
done

echo "Добавяне на семейства (families)..."
for n in "${families[@]}"; do
  add_if_missing "families" "$n" && ((added_fam++)) || true
done

echo "Добавяне на родове (genera)..."
for n in "${genera[@]}"; do
  add_if_missing "genera" "$n" && ((added_gen++)) || true
done

echo "Добавяне на видове (species)..."
for n in "${species[@]}"; do
  add_if_missing "species" "$n" && ((added_spec++)) || true
done

echo "Готово. Добавени: Отдели=$added_div, Класове=$added_class, Семейства=$added_fam, Родове=$added_gen, Видове=$added_spec"
