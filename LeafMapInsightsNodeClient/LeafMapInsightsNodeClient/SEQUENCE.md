# Sequence диаграма – LeafMapInsightsNodeClient (Node.js уеб сайт)

Node.js уеб сайтът е **апликацията за сайт**: Express обслужва статични страници от `public/` и подава API адресите чрез `GET /api-config`. Браузърът изпраща заявки директно към Auth API и Data API; JWT се пази в `localStorage`.

## Кратък поток

1. Браузър зарежда страница → вика `GET /api-config` → получава `{ apiBaseUrl, authApiBaseUrl }`.
2. Вход/регистрация: браузър → `POST api/auth/login` или `api/auth/register` → получава токен → записва в localStorage.
3. Списък дървета, детайли, карта, таксономия: браузър → `GET api/trees`, `api/trees/{id}`, `api/divisions`, ... с `Authorization: Bearer <token>`.
4. Добавяне на дърво: браузър → `GET api/divisions`, ... за dropdown-и, после `POST api/trees` с Bearer.
5. Изход: изтриване на токена от localStorage.

Пълната sequence диаграма (Mermaid) е в [../docs/sequence-diagrams.md](../docs/sequence-diagrams.md) (секция 2). Draw.io версия: [../docs/drawio/LeafMapInsightsNodeClient-sequence.drawio](../docs/drawio/LeafMapInsightsNodeClient-sequence.drawio).
