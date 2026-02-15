# Как да тестваш API (Приложение 1)

## 1. База данни

- Connection string е за **LocalDB** в `appsettings.json`.
- Ако нямаш LocalDB: инсталирай SQL Server Express LocalDB или промени connection string към твой SQL Server.

## 2. Стартиране на API-то

В папката на API проекта:

```bash
cd "c:\Users\titim\Desktop\LeafMap Insights - 3 different apps\ASPLeadMapInsightsAPI\ASPLeadMapInsightsAPI"
dotnet run
```

При първо стартиране ще се приложат миграциите и ще се добавят примерни данни (seed).  
API-то ще слуша на адрес от рода на: **https://localhost:7xxx** или **http://localhost:5xxx** (провери изхода в конзолата).

## 3. Тестване през Swagger (най-лесно)

1. Отвори в браузър: **https://localhost:7xxx/swagger** (замени с порта от конзолата; ако е HTTP, използвай **http://localhost:5xxx/swagger**).
2. В Swagger виждаш всички endpoints.

### Да видиш JSON отговор (без login)

- **GET /api/divisions** → Try it out → Execute. Ще видиш списък с един запис (напр. Magnoliophyta).
- **GET /api/families** → също връща JSON.
- **GET /api/trees** → двата seed дървета в JSON.
- **GET /api/trees?includeLookups=true** → дървета с пълни обекти Division, Family, Species и т.н.
- **GET /api/trees/WithinBounds?minLat=42.69&minLng=23.32&maxLat=42.70&maxLng=23.33** → дървета в този правоъгълник.

### Login и write операции

1. **POST /api/auth/register**  
   Request body (JSON):
   ```json
   {
     "email": "test@test.com",
     "password": "Test123!",
     "userName": "testuser"
   }
   ```  
   В отговора ще имаш `token`. Копирай го (само стойността на токена, без кавички).

2. В Swagger: натисни **Authorize** (горе вдясно), в полето напиши:  
   `Bearer <твоят_токен>`  
   (напр. `Bearer eyJhbGciOiJIUzI1NiIs...`).  
   Натисни Authorize и затвори.

3. Сега POST/PUT/DELETE ще работят:
   - **POST /api/trees** с JSON body (name, latitude, longitude, divisionId, taxonomyClassId, genusId, familyId, speciesId) – ще създаде ново дърво.
   - **DELETE /api/trees/1** – изтрива дърво с id 1 (ако съществува).

Ако не си натиснал Authorize и изпратиш POST/PUT/DELETE, ще получиш **401 Unauthorized**.

## 4. Тестване с curl (опционално)

База URL (смени порта): `https://localhost:7xxx`

```bash
# GET – виж JSON
curl -k https://localhost:7xxx/api/trees

# Register
curl -k -X POST https://localhost:7xxx/api/auth/register -H "Content-Type: application/json" -d "{\"email\":\"u@u.com\",\"password\":\"Pass123!\"}"

# Login (запази token от отговора)
curl -k -X POST https://localhost:7xxx/api/auth/login -H "Content-Type: application/json" -d "{\"email\":\"u@u.com\",\"password\":\"Pass123!\"}"

# GET с авторизация (смени TOKEN)
curl -k -H "Authorization: Bearer TOKEN" https://localhost:7xxx/api/trees
```

`-k` пропуска проверката на HTTPS сертификата за localhost.

## 5. Какво връща (JSON)

- **GET /api/divisions** → масив: `[{ "id": 1, "name": "Magnoliophyta" }]`
- **GET /api/trees** → масив от обекти с id, name, photoURL, description, latitude, longitude, divisionId, familyId, speciesId и т.н.
- **GET /api/trees?includeLookups=true** → същите дървета, но всяко има вложени обекти `division`, `family`, `species` и т.н.

Така проверяваш, че API-то връща JSON и че записите от seed данните се виждат.
