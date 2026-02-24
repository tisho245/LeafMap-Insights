# Sequence диаграма – LeafMapInsightsStaticWeb

Статичен HTML/JS клиент. Браузърът изпраща заявки директно към Auth API и Data API. JWT се пази в `localStorage`.

Пълният документ с всички три приложения: [../docs/sequence-diagrams.md](../docs/sequence-diagrams.md).

```mermaid
sequenceDiagram
    participant User as Потребител
    participant Browser as Браузър (app.js)
    participant AuthAPI as Auth API
    participant DataAPI as Data API

    Note over User,DataAPI: Проверка на връзка
    Browser->>DataAPI: GET api/divisions
    DataAPI-->>Browser: 200 OK / грешка

    Note over User,DataAPI: Вход / Регистрация
    User->>Browser: Login или Register
    Browser->>AuthAPI: POST api/auth/login или api/auth/register
    AuthAPI-->>Browser: { token }
    Browser->>Browser: localStorage (setToken)

    Note over User,DataAPI: Дървета и таксономия
    User->>Browser: Списък / Детайли / Таксономия
    Browser->>DataAPI: GET api/trees, api/trees/{id}, api/divisions, ...
    DataAPI-->>Browser: JSON
    Browser->>User: Рендира UI

    Note over User,DataAPI: Добавяне на дърво (с JWT)
    User->>Browser: Форма + Submit
    Browser->>DataAPI: GET api/divisions,... (dropdown-и)
    Browser->>DataAPI: POST api/trees [Authorization: Bearer]
    DataAPI-->>Browser: 201 Created
    Browser->>User: Успех
```
