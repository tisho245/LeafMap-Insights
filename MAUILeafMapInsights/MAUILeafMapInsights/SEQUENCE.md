# Sequence диаграма – MAUILeafMapInsights

MAUI приложение. Използва **LeafMapApiService** (HttpClient) и **AuthService** (SecureStorage за JWT). Login/Register → Auth API; останалите заявки → Data API с Bearer.

Пълен документ: [../../docs/sequence-diagrams.md](../../docs/sequence-diagrams.md).

```mermaid
sequenceDiagram
    participant User as Потребител
    participant MAUI as MAUI App
    participant AuthService as AuthService (SecureStorage)
    participant LeafMapApi as LeafMapApiService
    participant AuthAPI as Auth API
    participant DataAPI as Data API

    User->>MAUI: Стартиране (HomePage)
    MAUI->>AuthService: GetTokenAsync()
    MAUI-->>User: "Вход" / "Изход"

    Note over User,DataAPI: Вход / Регистрация
    User->>MAUI: LoginPage / RegisterPage
    MAUI->>LeafMapApi: LoginAsync() или RegisterAsync()
    LeafMapApi->>AuthAPI: POST api/auth/login или api/auth/register
    AuthAPI-->>LeafMapApi: { token }
    LeafMapApi-->>MAUI: LoginResponse
    MAUI->>AuthService: SetTokenAsync(token)
    MAUI-->>User: GoToAsync("//Trees")

    Note over User,DataAPI: Дървета, детайли, таксономия
    User->>MAUI: Trees / детайли / Taxonomy
    MAUI->>LeafMapApi: GetTreesAsync(), GetTreeAsync(id), GetDivisionsAsync(), ...
    LeafMapApi->>AuthService: GetTokenAsync()
    LeafMapApi->>DataAPI: GET api/trees, api/divisions, ... [Bearer]
    DataAPI-->>LeafMapApi: данни
    LeafMapApi-->>MAUI: DTO списъци
    MAUI-->>User: UI

    Note over User,DataAPI: Добавяне на дърво
    User->>MAUI: "Добави дърво"
    MAUI->>AuthService: IsLoggedInAsync()
    alt не е логнат
        MAUI-->>User: GoToAsync("//Login")
    else логнат
        MAUI->>LeafMapApi: GetDivisionsAsync(), ... затем CreateTreeAsync(tree)
        LeafMapApi->>DataAPI: GET затем POST api/trees [Bearer]
        DataAPI-->>LeafMapApi: 201
        MAUI-->>User: Успех
    end

    User->>MAUI: Изход
    MAUI->>AuthService: RemoveTokenAsync()
    MAUI-->>User: "Вход"
```
