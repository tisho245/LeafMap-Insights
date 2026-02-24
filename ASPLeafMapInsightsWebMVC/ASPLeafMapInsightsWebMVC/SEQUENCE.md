# Sequence диаграма – ASPLeafMapInsightsWebMVC

ASP.NET MVC приложение. Потребителят комуникира само с MVC; MVC извиква Auth API и Data API. JWT се пази в **Session**.

Пълен документ: [../../docs/sequence-diagrams.md](../../docs/sequence-diagrams.md).

```mermaid
sequenceDiagram
    participant User as Потребител
    participant MVC as ASP.NET MVC
    participant AuthAPI as Auth API
    participant DataAPI as Data API

    User->>MVC: GET / (Home)
    MVC-->>User: View Index

    Note over User,DataAPI: Вход / Регистрация
    User->>MVC: GET /Auth/Login или /Auth/Register
    MVC-->>User: View (форма)
    User->>MVC: POST (email, password, userName)
    MVC->>AuthAPI: POST api/auth/login или api/auth/register
    AuthAPI-->>MVC: { token }
    MVC->>MVC: Session["Token"] = token
    MVC-->>User: Redirect

    Note over User,DataAPI: Дървета, Таксономия, Карта
    User->>MVC: GET /Trees, /Taxonomy, /Map
    MVC->>DataAPI: GET api/trees, api/divisions, ... [Bearer Session["Token"]]
    DataAPI-->>MVC: данни
    MVC-->>User: View

    Note over User,DataAPI: Създаване на дърво (логнат)
    User->>MVC: GET /Trees/Create
    MVC->>DataAPI: GET api/divisions, api/species, ...
    MVC-->>User: Форма с dropdown-и
    User->>MVC: POST /Trees/Create
    MVC->>DataAPI: POST api/trees [Bearer]
    DataAPI-->>MVC: 201
    MVC-->>User: RedirectToAction("Index")

    User->>MVC: POST /Auth/Logout
    MVC->>MVC: Session.Remove("Token")
    MVC-->>User: Redirect Home
```
