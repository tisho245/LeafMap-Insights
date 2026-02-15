# Как да пуснеш Web приложението (Приложение 2)

## 1. Първо стартирай API-то (Приложение 1)

В папката на API проекта:
```bash
cd "c:\Users\titim\Desktop\LeafMap Insights - 3 different apps\ASPLeadMapInsightsAPI\ASPLeadMapInsightsAPI"
dotnet run
```
Остави го да работи. Забележи на кой URL е (напр. `https://localhost:7234`).

## 2. Ако API-то е на друг порт

В **ASPLeafMapInsightsWebMVC/appsettings.json** (или **appsettings.Development.json**) промени:
```json
"ApiBaseUrl": "https://localhost:7234"
```
на същия URL, на който ти показва че работи API-то.

## 3. Стартирай Web приложението

В папката на MVC проекта:
```bash
cd "c:\Users\titim\Desktop\LeafMap Insights - 3 different apps\ASPLeafMapInsightsWebMVC\ASPLeafMapInsightsWebMVC"
dotnet run
```
Отвори в браузър адреса от конзолата (напр. `https://localhost:7134` или `http://localhost:5049`).

## 4. Какво да пробваш

- **Начало** – начална страница с линкове.
- **Дървета** – списък от API (seed данните – два дъба).
- **Карта** – списък с координати (данните от API).
- **Таксономия** – отдели, класове, семейства, родове, видове от API.
- **Добави дърво** – формуляр; работи само ако си **логнат** (Вход → регистрация или login).
- **Вход / Регистрация** – изпращат заявки към API, след успех JWT се пази в сесията и можеш да създаваш дървета.

Ако „Дървета“ или „Таксономия“ са празни, провери дали API-то работи и дали **ApiBaseUrl** в appsettings съвпада с URL на API-то.
