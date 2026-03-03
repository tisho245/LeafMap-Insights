#!/usr/bin/env bash
# Изпълнява seed_taxonomy.sql срещу SQL Server.
# Нужен: sqlcmd (SQL Server Command Line Tools) или Azure Data Studio / sqlcmd в Docker.
#
# Употреба:
#   ./run_seed_taxonomy.sh
#   SQL_SERVER=localhost SQL_USER=sa SQL_PASS=secret ./run_seed_taxonomy.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SQL_FILE="${SCRIPT_DIR}/seed_taxonomy.sql"

SQL_SERVER="${SQL_SERVER:-localhost,1433}"
SQL_USER="${SQL_USER:-sa}"
SQL_PASS="${SQL_PASS:-SuperAdmin2026}"
SQL_DB="${SQL_DB:-LeafMapInsights}"

if [[ ! -f "$SQL_FILE" ]]; then
  echo "Файлът не е намерен: $SQL_FILE"
  exit 1
fi

if command -v sqlcmd &>/dev/null; then
  # sqlcmd (mssql-tools18 или по-стара версия)
  sqlcmd -S "$SQL_SERVER" -d "$SQL_DB" -U "$SQL_USER" -P "$SQL_PASS" -C -i "$SQL_FILE"
  echo "Готово."
elif command -v sqlcmd.exe &>/dev/null; then
  sqlcmd.exe -S "$SQL_SERVER" -d "$SQL_DB" -U "$SQL_USER" -P "$SQL_PASS" -C -i "$SQL_FILE"
  echo "Готово."
else
  echo "sqlcmd не е намерен. Инсталирайте Microsoft SQL Server Command Line Tools (mssql-tools18) или изпълнете ръчно:"
  echo ""
  echo "  sqlcmd -S ${SQL_SERVER} -d ${SQL_DB} -U ${SQL_USER} -P <парола> -C -i seed_taxonomy.sql"
  echo ""
  echo "Или с Docker (ако SQL Server работи в контейнер):"
  echo "  docker exec -i <container_name> /opt/mssql-tools18/bin/sqlcmd -S localhost -d ${SQL_DB} -U sa -P \"\${SQL_PASS}\" -C < seed_taxonomy.sql"
  exit 1
fi
