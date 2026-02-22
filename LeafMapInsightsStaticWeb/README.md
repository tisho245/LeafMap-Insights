# LeafMap Insights – статичен уеб клиент (HTML/JS/CSS)

Това е самостоятелен клиент към LeafMap Insights API – само HTML, JavaScript и CSS, без сървърна част.

## Конфигурация

1. Пуснете **API** (ASPLeadMapInsightsAPI) на сървъра.
2. Отворете `app.js` и задайте **API_BASE_URL** според сървъра:
   - Локално: `https://localhost:7234` или `http://localhost:5202`
   - От друг компютър: `http://IP-на-сървъра:порт`

## Стартиране

- Отворете `index.html` директно в браузър (file://), **или**
- Пуснете локален сървър в папката, напр.:
  - `npx serve .`
  - `python -m http.server 8080`

**Важно:** Ако API е на `https` с самоподписан сертификат, браузърът може да блокира заявките. За разработка може да се използва HTTP или да се довери сертификата.

## CORS

API трябва да разрешава заявки от origin-а, от който отваряте страницата (напр. `http://localhost:8080` или `file://`). В API е конфигуриран CORS; при нужда добавете конкретния origin в политиката.

## Функции

- **Вход / Регистрация** – JWT се пази в `localStorage`
- **Списък дървета** – GET `api/trees?includeLookups=true`
- **Детайли за дърво** – GET `api/trees/{id}`
- **Таксономия** – Divisions, TaxonomyClasses, Families, Genera, Species
- **Добавяне на дърво** – POST `api/trees` (изисква логнат потребител)

## Файлове

- `index.html` – структура и разметка
- `app.js` – заявки към API и логика на интерфейса
- `styles.css` – стилове (тема в земни тонове / природа)
