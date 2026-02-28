/**
 * LeafMap Insights – Node.js клиент сървър
 * Обслужва статичните страници и подава API адреси чрез /api-config.
 * Модулност: адресите идват от env – реално IP, без localhost.
 * Задай API_HOST=IP_НА_СЪРВЪРА или API_BASE_URL + AUTH_API_BASE_URL в .env.
 */
require('dotenv').config();
const express = require('express');
const path = require('path');

const app = express();
const PORT = process.env.PORT || 3000;
const HOST = process.env.HOST || '0.0.0.0';

// Един хост за всички API (модулност) или отделни URL-и
const API_HOST = process.env.API_HOST || '';
const API_BASE_URL = process.env.API_BASE_URL || (API_HOST ? `http://${API_HOST}:5202` : '');
const AUTH_API_BASE_URL = process.env.AUTH_API_BASE_URL || (API_HOST ? `http://${API_HOST}:5203` : '');

app.use(express.static(path.join(__dirname, 'public')));

app.get('/api-config', (req, res) => {
  res.setHeader('Content-Type', 'application/json');
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.json({ apiBaseUrl: API_BASE_URL, authApiBaseUrl: AUTH_API_BASE_URL });
});

if (!API_BASE_URL || !AUTH_API_BASE_URL) {
  console.error('Задай API_HOST=IP_НА_СЪРВЪРА или API_BASE_URL и AUTH_API_BASE_URL в .env');
  process.exit(1);
}

app.listen(PORT, HOST, () => {
  console.log(`LeafMap Node client: http://${HOST === '0.0.0.0' ? 'localhost' : HOST}:${PORT}`);
  console.log(`Data API: ${API_BASE_URL}, Auth API: ${AUTH_API_BASE_URL}`);
});
