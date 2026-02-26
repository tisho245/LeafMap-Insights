# Sequence диаграми – LeafMap Insights

Този документ съдържа sequence диаграми за трите клиентски приложения, които комуникират с **LeafMap Insights Auth API** и **LeafMap Insights Data API**. За обзор **през коя апликация какво можем да правим** вижте [use-case-diagram.md](use-case-diagram.md).

**PlantUML версия:** Всички диаграми са налични и като PlantUML в [sequence-diagrams.puml](sequence-diagrams.puml) (Node.js, Auth API, Data API, MAUI + обзор).

---

## 1. LeafMapInsightsStaticWeb (статичен HTML/JS клиент)

Браузърът зарежда статични страници; JavaScript изпраща заявки директно към Auth API и Data API. JWT се пази в `localStorage`.

```mermaid
sequenceDiagram
    participant User as Потребител
    participant Browser as Браузър (app.js)
    participant AuthAPI as Auth API
    participant DataAPI as Data API

    Note over User,DataAPI: Проверка на връзка при зареждане
    Browser->>DataAPI: GET api/divisions (проверка на API)
    DataAPI-->>Browser: 200 OK / грешка

    Note over User,DataAPI: Вход
    User->>Browser: Въвежда email/парола (Login)
    Browser->>AuthAPI: POST api/auth/login { email, password }
    AuthAPI-->>Browser: { token, email, expiresAt }
    Browser->>Browser: setToken() → localStorage
    Browser->>Browser: updateAuthUI()

    Note over User,DataAPI: Регистрация
    User->>Browser: Въвежда email/парола/username (Register)
    Browser->>AuthAPI: POST api/auth/register { email, password, userName }
    AuthAPI-->>Browser: { token, email, expiresAt }
    Browser->>Browser: setToken() → localStorage

    Note over User,DataAPI: Списък дървета
    User->>Browser: Отваря начална / дървета
    Browser->>DataAPI: GET api/trees?includeLookups=true [Authorization: Bearer token]
    DataAPI-->>Browser: [ { id, name, species, ... } ]
    Browser->>User: Рендира карти (renderTreeCard)

    Note over User,DataAPI: Детайли за дърво
    User->>Browser: Клик "Детайли" (tree.html?id=...)
    Browser->>DataAPI: GET api/trees/{id}?includeLookups=true [Bearer]
    DataAPI-->>Browser: { id, name, species, genus, family, ... }
    Browser->>User: Показва детайли

    Note over User,DataAPI: Таксономия
    Browser->>DataAPI: GET api/divisions, api/taxonomyclasses, api/families, api/genera, api/species
    DataAPI-->>Browser: списъци
    Browser->>User: Показва таксономия

    Note over User,DataAPI: Добавяне на дърво (само при логнат потребител)
    User->>Browser: Попълва форма и Submit
    Browser->>DataAPI: GET api/divisions, ... (за dropdown-и)
    DataAPI-->>Browser: списъци
    Browser->>DataAPI: POST api/trees { name, latitude, longitude, speciesId, ... } [Bearer]
    DataAPI-->>Browser: 201 Created
    Browser->>User: "Дървото е добавено успешно."
```

---

## 2. LeafMapInsightsNodeClient (Node.js уеб сайт)

Уеб сайтът е **Node.js** приложение (Express). Сървърът обслужва статични страници от `public/` и подава API адресите чрез `GET /api-config`. Браузърът след това изпраща заявки **директно** към Auth API и Data API; JWT се пази в `localStorage` (също като Static Web).

```mermaid
sequenceDiagram
    participant User as Потребител
    participant Browser as Браузър (app.js)
    participant Node as Node сървър
    participant AuthAPI as Auth API
    participant DataAPI as Data API

    Note over User,DataAPI: Конфигурация при зареждане
    Browser->>Node: GET /api-config
    Node-->>Browser: { apiBaseUrl, authApiBaseUrl }
    Browser->>DataAPI: GET api/divisions (проверка на API)
    DataAPI-->>Browser: 200 OK / грешка

    Note over User,DataAPI: Вход
    User->>Browser: Въвежда email/парола (Login)
    Browser->>AuthAPI: POST api/auth/login { email, password }
    AuthAPI-->>Browser: { token, email, expiresAt }
    Browser->>Browser: setToken() → localStorage

    Note over User,DataAPI: Регистрация
    User->>Browser: Register (email, password, userName)
    Browser->>AuthAPI: POST api/auth/register { email, password, userName }
    AuthAPI-->>Browser: { token }
    Browser->>Browser: setToken() → localStorage

    Note over User,DataAPI: Списък дървета / Детайли / Карта / Таксономия
    User->>Browser: Навигация (trees, tree, taxonomy, add-tree)
    Browser->>DataAPI: GET api/trees, api/trees/{id}, api/divisions, ... [Bearer]
    DataAPI-->>Browser: JSON
    Browser->>User: Рендира страница

    Note over User,DataAPI: Добавяне на дърво (при логнат потребител)
    User->>Browser: Попълва форма и Submit
    Browser->>DataAPI: GET api/divisions, ... затем POST api/trees [Bearer]
    DataAPI-->>Browser: 201 Created
    Browser->>User: Успех

    Note over User,DataAPI: Изход
    User->>Browser: Изход
    Browser->>Browser: removeItem(TOKEN_KEY)
```

---

## 3. MAUILeafMapInsights (MAUI мобилно/десктоп приложение)

Приложението използва **LeafMapApiService** (HttpClient) и **AuthService** (SecureStorage за JWT). Login/Register отиват към Auth API; всички останали заявки към Data API с Bearer токен.

```mermaid
sequenceDiagram
    participant User as Потребител
    participant MAUI as MAUI App
    participant AuthService as AuthService (SecureStorage)
    participant LeafMapApi as LeafMapApiService
    participant AuthAPI as Auth API
    participant DataAPI as Data API

    Note over User,DataAPI: Начална страница (HomePage)
    User->>MAUI: Отваря приложението
    MAUI->>AuthService: GetTokenAsync()
    AuthService-->>MAUI: token / null
    MAUI->>User: Показва "Вход" или "Изход"

    Note over User,DataAPI: Вход
    User->>MAUI: Клик "Вход" → LoginPage
    User->>MAUI: Въвежда email/парола
    MAUI->>LeafMapApi: LoginAsync(email, password)
    LeafMapApi->>AuthAPI: POST {AuthBaseUrl}/api/auth/login { email, password }
    AuthAPI-->>LeafMapApi: { token, email, expiresAt }
    LeafMapApi-->>MAUI: LoginResponse
    MAUI->>AuthService: SetTokenAsync(token)
    MAUI-->>User: Shell.GoToAsync("//Trees")

    Note over User,DataAPI: Регистрация
    User->>MAUI: RegisterPage → email, password, userName
    MAUI->>LeafMapApi: RegisterAsync(email, password, userName)
    LeafMapApi->>AuthAPI: POST api/auth/register
    AuthAPI-->>LeafMapApi: { token }
    LeafMapApi-->>MAUI: LoginResponse
    MAUI->>AuthService: SetTokenAsync(token)
    MAUI-->>User: GoToAsync("//Trees")

    Note over User,DataAPI: Списък дървета
    User->>MAUI: Trees (списък)
    MAUI->>LeafMapApi: GetTreesAsync(includeLookups: true)
    LeafMapApi->>AuthService: GetTokenAsync()
    AuthService-->>LeafMapApi: token
    LeafMapApi->>DataAPI: GET api/trees?includeLookups=true [Authorization: Bearer token]
    DataAPI-->>LeafMapApi: List<TreeDto>
    LeafMapApi-->>MAUI: данни
    MAUI-->>User: Показва списък

    Note over User,DataAPI: Детайли за дърво
    User->>MAUI: Избор на дърво
    MAUI->>LeafMapApi: GetTreeAsync(id)
    LeafMapApi->>AuthService: GetTokenAsync()
    LeafMapApi->>DataAPI: GET api/trees/{id}?includeLookups=true [Bearer]
    DataAPI-->>LeafMapApi: TreeDto
    LeafMapApi-->>MAUI: данни
    MAUI-->>User: Детайли

    Note over User,DataAPI: Добавяне на дърво
    User->>MAUI: Клик "Добави дърво"
    MAUI->>AuthService: IsLoggedInAsync()
    AuthService-->>MAUI: true/false
    alt не е логнат
        MAUI-->>User: GoToAsync("//Login")
    else логнат
        MAUI->>MAUI: AddTreePage
        MAUI->>LeafMapApi: GetDivisionsAsync(), GetTaxonomyClassesAsync(), GetGeneraAsync(), GetFamiliesAsync(), GetSpeciesAsync()
        LeafMapApi->>DataAPI: GET api/divisions, api/taxonomyclasses, ... [Bearer]
        DataAPI-->>LeafMapApi: списъци
        LeafMapApi-->>MAUI: данни за Picker-и
        User->>MAUI: Попълва форма и Submit
        MAUI->>LeafMapApi: CreateTreeAsync(tree)
        LeafMapApi->>DataAPI: POST api/trees { name, latitude, ... } [Bearer]
        DataAPI-->>LeafMapApi: 201
        LeafMapApi-->>MAUI: true
        MAUI-->>User: Успех / навигация назад
    end

    Note over User,DataAPI: Таксономия
    User->>MAUI: Taxonomy
    MAUI->>LeafMapApi: GetDivisionsAsync(), GetTaxonomyClassesAsync(), ... (или отделна страница)
    LeafMapApi->>DataAPI: GET api/divisions, ... [Bearer]
    DataAPI-->>MAUI: списъци
    MAUI-->>User: Показва таксономия

    Note over User,DataAPI: Изход
    User->>MAUI: Клик "Изход"
    MAUI->>AuthService: RemoveTokenAsync()
    MAUI->>User: AuthButton.Text = "Вход"
```

---

## Обобщение на компонентите

| Проект | Клиент | Съхранение на JWT | Auth извиквания | Data API извиквания |
|--------|--------|-------------------|------------------|---------------------|
| **LeafMapInsightsStaticWeb** | Браузър (HTML/JS) | localStorage | директно към Auth API | директно с Bearer |
| **LeafMapInsightsNodeClient** (Node.js сайт) | Браузър → Node за /api-config, после директно | localStorage | директно към Auth API | директно с Bearer |
| **MAUILeafMapInsights** | MAUI приложение | SecureStorage | LeafMapApiService → Auth API | LeafMapApiService → Data API с Bearer от AuthService |

Всички три приложения използват един и същ **Auth API** (`api/auth/login`, `api/auth/register`) и един и същ **Data API** (trees, divisions, taxonomyclasses, families, genera, species). Уеб сайтът е **Node.js** (LeafMapInsightsNodeClient); ASP.NET MVC е премахнат.
