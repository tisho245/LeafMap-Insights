# LeafMap Insights – Docker

Всички услуги могат да се пуснат в отделни контейнери чрез Docker Compose.

## Изисквания

- Docker и Docker Compose
- Портове 80, 1433, 3000, 5000, 5202, 5203, 8080 свободни (или промени в `docker-compose.yml`)

## Стартиране

От **корена на репо** (папката, в която е `docker-compose.yml`):

```bash
docker-compose up -d --build
```

Първото стартиране ще сглоби образчета и ще пусне контейнерите. SQL Server може да отнеме 20–30 секунди да стане готов. Ако **dataapi** или **authapi** паднат с грешка за връзка с БД, рестартирайте ги:

```bash
docker-compose restart dataapi authapi
```

## Услуги и портове

| Услуга      | URL (на хоста)        | Описание                    |
|------------|------------------------|-----------------------------|
| Data API   | http://localhost:5202  | Дървета, таксономия         |
| Auth API   | http://localhost:5203  | Вход, регистрация           |
| Node client| http://localhost:3000  | Node.js уеб сайт (апликация за сайт) |
| Static web | http://localhost:8080  | Статичен HTML/JS клиент     |
| SQL Server | localhost:1433         | База данни (вътрешна)       |

## Конфигурация

- **Парола за SQL**: в `docker-compose.yml` е зададена `MSSQL_SA_PASSWORD: "YourStrong!Pass123"`. За промяна сменете я и стойността в `ConnectionStrings__DefaultConnection` за **dataapi** и **authapi**.
- **JWT ключ**: `Jwt__Key` трябва да е един и същ за **dataapi** и **authapi** (и да е поне 32 символа).
- **Static web**: при отваряне на http://localhost:8080 задайте на началната страница Data API = `http://localhost:5202` и Auth API = `http://localhost:5203`.
- **Node client**: получава API URL от сървъра (`/api-config`), който вече е конфигуриран с localhost:5202 и :5203 в compose.

## Спиране

```bash
docker-compose down
```

Данните на SQL Server остават в volume `sqlserver_data`. За пълно изтриване и данните:

```bash
docker-compose down -v
```

## Само отделни услуги

Може да билдвате и пускате само някои от услугите, например:

```bash
docker-compose up -d sqlserver dataapi authapi
```

След това отваряйте Node/Static от хоста към съответните портове.
