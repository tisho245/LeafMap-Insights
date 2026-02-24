/**
 * LeafMap Insights – Node.js клиент сървър
 * Обслужва статичните страници и подава API_BASE_URL чрез /api-config (от .env).
 * Пуснете: npm install && npm start  →  http://localhost:3000
 */
require('dotenv').config();
const express = require('express');
const path = require('path');

const app = express();
const PORT = process.env.PORT || 3000;
const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7234';
const AUTH_API_BASE_URL = process.env.AUTH_API_BASE_URL || 'https://localhost:7240';

app.use(express.static(path.join(__dirname, 'public')));

app.get('/api-config', (req, res) => {
  res.json({ apiBaseUrl: API_BASE_URL, authApiBaseUrl: AUTH_API_BASE_URL });
});

app.listen(PORT, () => {
  console.log(`LeafMap Node client: http://localhost:${PORT}`);
  console.log(`Data API: ${API_BASE_URL}, Auth API: ${AUTH_API_BASE_URL}`);
});
