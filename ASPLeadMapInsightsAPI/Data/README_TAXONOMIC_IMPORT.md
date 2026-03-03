# Импорт на таксономични данни

Файлът `taxonomic_import.json` съдържа видове с пълна йерархия: **division** → **taxonomyClass** → **family** → **genus** → **species**.

---

## SQL заявки + bash (директно в БД)

Файлът **`seed_taxonomy.sql`** съдържа идемпотентни `INSERT ... WHERE NOT EXISTS` за всички отдели, класове, семейства, родове и видове от данните по-горе.

**Еднократна bash команда** (нужен е `sqlcmd` – SQL Server Command Line Tools):

```bash
cd "ASPLeadMapInsightsAPI/Data"
sqlcmd -S localhost,1433 -d LeafMapInsights -U sa -P SuperAdmin2026 -C -i seed_taxonomy.sql
```

С променливи за сървър/потребител/парола:

```bash
SQL_SERVER=localhost,1433 SQL_USER=sa SQL_PASS=твоята_парола sqlcmd -S "$SQL_SERVER" -d LeafMapInsights -U "$SQL_USER" -P "$SQL_PASS" -C -i seed_taxonomy.sql
```

Или изпълни скрипта (извиква `sqlcmd` с горните параметри):

```bash
chmod +x run_seed_taxonomy.sh
./run_seed_taxonomy.sh
```

Променливи на средата за `run_seed_taxonomy.sh`: `SQL_SERVER`, `SQL_USER`, `SQL_PASS`, `SQL_DB` (по подразбиране `LeafMapInsights`). Флагът `-C` означава Trust Server Certificate (за локален/Dev сървър).

### Дървета с български имена (едни и същи координати)

Файлът **`seed_trees.sql`** вкарва дървета с имената в „зеленото“ (Ружа, Секвоя, Смрика, Чинар, Акация, Клен, Явор и др.), всички с координати **42.656683759116866, 24.745867485545126**. За Чинар е зададено пълното описание от заданието; за останалите `Description` е NULL. Първо трябва да е изпълнен `seed_taxonomy.sql` (таксономията трябва да съществува).

Еднократно изпълнение на таксономия + дървета:

```bash
cd "ASPLeadMapInsightsAPI/Data"
./run_seed_all.sh
```

Или ръчно в правилния ред:

```bash
sqlcmd -S localhost,1433 -d LeafMapInsights -U sa -P SuperAdmin2026 -C -i seed_taxonomy.sql
sqlcmd -S localhost,1433 -d LeafMapInsights -U sa -P SuperAdmin2026 -C -i seed_trees.sql
```

---

Редактирайте `taxonomic_import.json` според документите от папката „Растителни видове в Заимов“ (Класификация на организмите.docx и отделните .doc за всеки вид). Форматът на всеки запис е:

```json
{
  "division": "Magnoliophyta",
  "taxonomyClass": "Magnoliopsida",
  "family": "Rosaceae",
  "genus": "Rosa",
  "species": "Rosa canina"
}
```

## Bash скрипт (препоръчително)

Нужни: **curl**, **jq**. API-то и Auth API трябва да са пуснати. Потребителят трябва да е с роля **Admin**.

```bash
cd ASPLeadMapInsightsAPI/Data
chmod +x import_taxonomy.sh
ADMIN_USER=admin ADMIN_PASS=вашата_парола ./import_taxonomy.sh
```

С друг JSON файл:

```bash
ADMIN_USER=admin ADMIN_PASS=secret ./import_taxonomy.sh /път/към/taxonomic_import.json
```

Променливи на средата (по избор):

- `AUTH_API_URL` – по подразбиране `http://localhost:5203`
- `API_BASE_URL` – по подразбиране `http://localhost:5202`

Импортът е идемпотентен – добавя само липсващи имена.

---

## C# конзолен скрипт (алтернатива)

От папката на решението (ASPLeadMapInsightsAPI):

```bash
dotnet run --project TaxonomyImport
```

С опционален път към JSON файл:

```bash
dotnet run --project TaxonomyImport -- "C:\път\към\taxonomic_import.json"
```

Connection string се взима от `ASPLeadMapInsightsAPI/appsettings.json` или от променливата на средата `ConnectionStrings__DefaultConnection`.
