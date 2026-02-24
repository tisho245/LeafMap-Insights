# LeafMap Insights – Node.js клиент

Уеб клиент за LeafMap Insights API, обслужван от Express. API адресът се задава в `.env` и се подава на браузъра чрез `/api-config`.

## Изисквания

- Node.js 18+
- LeafMap Insights API да работи на зададения адрес

## Инсталация и стартиране

```bash
cd LeafMapInsightsNodeClient
npm install
cp .env.example .env
# Редактирайте .env и задайте API_BASE_URL (напр. https://localhost:7234 или http://localhost:5202)
npm start
```

Отворете в браузър: **http://localhost:3000**

## Конфигурация

В `.env`:

- **API_BASE_URL** – Data API (дървета, таксономия), напр. `https://localhost:7234`
- **AUTH_API_BASE_URL** – Auth API (вход, регистрация), напр. `https://localhost:7240`. При един сървър може да съвпада с API_BASE_URL.
- **PORT** – порт на Node сървъра (по подразбиране 3000)

При зареждане клиентът вика `GET /api-config` и получава `apiBaseUrl` и `authApiBaseUrl`; вход/регистрация отиват към Auth API, останалото към Data API.

## Структура

- `server.js` – Express сървър, статични файлове от `public/`, endpoint `/api-config`
- `public/` – HTML страници (index, trees, tree, taxonomy, add-tree, login, register), `app.js`, `styles.css`
