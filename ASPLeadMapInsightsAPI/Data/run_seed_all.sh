#!/usr/bin/env bash
# Първо вкарва таксономия (seed_taxonomy.sql), после дървета с български имена (seed_trees.sql).
# Нужен: sqlcmd
#
# Употреба:
#   SQL_SERVER=localhost,1433 SQL_USER=sa SQL_PASS=SuperAdmin2026 ./run_seed_all.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SQL_SERVER="${SQL_SERVER:-localhost,1433}"
SQL_USER="${SQL_USER:-sa}"
SQL_PASS="${SQL_PASS:-SuperAdmin2026}"
SQL_DB="${SQL_DB:-LeafMapInsights}"

RUN_CMD=""
if command -v sqlcmd &>/dev/null; then
  RUN_CMD="sqlcmd"
elif command -v sqlcmd.exe &>/dev/null; then
  RUN_CMD="sqlcmd.exe"
fi

if [[ -z "$RUN_CMD" ]]; then
  echo "sqlcmd не е намерен. Инсталирайте SQL Server Command Line Tools."
  exit 1
fi

echo "1/2 Таксономия..."
$RUN_CMD -S "$SQL_SERVER" -d "$SQL_DB" -U "$SQL_USER" -P "$SQL_PASS" -C -i "$SCRIPT_DIR/seed_taxonomy.sql"
echo "2/2 Дървета..."
$RUN_CMD -S "$SQL_SERVER" -d "$SQL_DB" -U "$SQL_USER" -P "$SQL_PASS" -C -i "$SCRIPT_DIR/seed_trees.sql"
echo "Готово."
