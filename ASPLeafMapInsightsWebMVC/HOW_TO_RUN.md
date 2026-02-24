# Как да пуснеш Web приложението (MVC) с двата API сървъра

## Три приложения в едно решение

В папката **ASPLeadMapInsightsAPI** има решение `ASPLeadMapInsightsAPI.sln` с **три проекта**:

1. **ASPLeadMapInsightsAPI** – Data API (дървета, таксономия). Порт: `https://localhost:7234` / `http://localhost:5202`
2. **LeafMapInsightsAuthAPI** – Auth API (вход, регистрация, издаване на JWT). Порт: `https://localhost:7240` / `http://localhost:5203`
3. **ASPLeafMapInsightsWebMVC** – уеб клиент (MVC). Порт: напр. `https://localhost:7134` / `http://localhost:5049`

## 1. Стартирай двата API сървъра

**Терминал 1 – Data API:**
```bash
cd "ASPLeadMapInsightsAPI\ASPLeadMapInsightsAPI"
dotnet run
```
Остави го да работи (напр. `https://localhost:7234`).

**Терминал 2 – Auth API:**
```bash
cd "LeafMapInsightsAuthAPI\LeafMapInsightsAuthAPI"
dotnet run
```
Остави го да работи (напр. `https://localhost:7240`).

И двата API използват **същата база данни** (ConnectionString в appsettings). Миграциите се прилагат при стартиране на всеки от тях.

## 2. Конфигурация на MVC

В **ASPLeafMapInsightsWebMVC/appsettings.json** (или **appsettings.Development.json**):

- **ApiBaseUrl** – адрес на **Data API** (дървета, таксономия): напр. `https://localhost:7234`
- **AuthApiBaseUrl** – адрес на **Auth API** (вход/регистрация): напр. `https://localhost:7240`

Ако пуснеш само един API (като преди), остави **AuthApiBaseUrl** празен или изтрий го – тогава MVC използва **ApiBaseUrl** и за логин/регистрация.

## 3. Стартирай MVC приложението

**Терминал 3:**
```bash
cd "ASPLeafMapInsightsWebMVC\ASPLeafMapInsightsWebMVC"
dotnet run
```
Отвори в браузър адреса от конзолата (напр. `https://localhost:7134` или `http://localhost:5049`).

## 4. Какво да пробваш

- **Начало** – начална страница с линкове.
- **Дървета** – списък от Data API.
- **Карта** – дървета с координати от Data API.
- **Таксономия** – отдели, класове, семейства, родове, видове от Data API.
- **Вход / Регистрация** – заявки към **Auth API**; след успех JWT се пази в сесията.
- **Добави дърво** – изисква логнат потребител; заявките отиват към **Data API** с Bearer токена.

Ако „Дървета“ или „Таксономия“ са празни, провери дали **Data API** работи и дали **ApiBaseUrl** съвпада. Ако вход/регистрация не работят, провери дали **Auth API** работи и дали **AuthApiBaseUrl** е зададен.
