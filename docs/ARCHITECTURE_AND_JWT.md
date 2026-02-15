# LeafMap Insights – Архитектура, контролери и JWT

## 1. Как се свързват трите приложения

```
                    ┌─────────────────────────────────────┐
                    │     ПРИЛОЖЕНИЕ 1: Backend API        │
                    │     (ASPLeadMapInsightsAPI)           │
                    │  • База данни (SQL Server/LocalDB)   │
                    │  • Entity Framework, модели          │
                    │  • CRUD контролери                    │
                    │  • Auth: Login/Register → издава JWT │
                    │  • Валидира JWT при всяка заявка      │
                    └─────────────────┬───────────────────┘
                                      │
                    HTTP/HTTPS (JSON) │
                    ┌─────────────────┴───────────────────┐
                    │                                     │
        ┌───────────▼───────────┐           ┌──────────────▼──────────────┐
        │ ПРИЛОЖЕНИЕ 2:        │           │ ПРИЛОЖЕНИЕ 3:               │
        │ Web MVC              │           │ Mobile (MAUI, Android)      │
        │ (ASPLeafMapInsights   │           │ (MAUILeafMapInsights)      │
        │  WebMVC)             │           │                             │
        │ • Няма БД            │           │ • Няма БД                   │
        │ • HttpClient → API   │           │ • HttpClient → API          │
        │ • JWT в сесия        │           │ • JWT в SecureStorage        │
        │ • Razor Views        │           │ • XAML страници              │
        └─────────────────────┘           └─────────────────────────────┘
```

- **Един източник на истината**: само API-то чете и записва в базата. Web и Mobile **никога** не ползват DbContext или собствена БД за данните за дървета/таксономия.
- **Един и същ договор**: и двете клиентски приложения викат същите endpoints (например `GET /api/trees`, `POST /api/auth/login`) и работят с един и същ JSON формат.
- **Конфигурируем URL**: Web има `ApiBaseUrl` в `appsettings.json`; Mobile има `ApiSettings.BaseUrl` в кода. Те трябва да сочат към адреса, на който работи API-то (напр. `https://localhost:7234` или от емулатор `http://10.0.2.2:5202`).

---

## 2. Как работят контролерите

### 2.1. В API (Приложение 1)

**Роля**: единственото приложение, което говори с БД. Контролерите получават заявки, четат/пишат чрез `LeafMapDbContext`, връщат JSON.

| Файл | Роля |
|------|------|
| `Controllers/DivisionsController.cs` | CRUD за таблица Divisions. GET – без авторизация; POST, PUT, DELETE – с `[Authorize]`. |
| `Controllers/TaxonomyClassesController.cs` | Също за TaxonomyClasses. |
| `Controllers/GeneraController.cs` | Също за Genera. |
| `Controllers/FamiliesController.cs` | Също за Families. |
| `Controllers/SpeciesController.cs` | Също за Species. |
| `Controllers/TreesController.cs` | CRUD за Trees + `GET /api/trees/WithinBounds?minLat=...` за филтриране по карта. POST/PUT/DELETE – с `[Authorize]`. |
| `Controllers/AuthController.cs` | `POST /api/auth/register` и `POST /api/auth/login`. Не изискват токен; връщат JWT в тялото на отговора. |

**Типичен поток в API контролер**:

1. Заявката влиза в action (напр. `GetAll()`).
2. Контролерът използва инжектирания `LeafMapDbContext` (или друг сервис).
3. Прави заявка към БД (напр. `_context.Trees.Include(...).ToListAsync()`).
4. Връща `Ok(list)` → ASP.NET Core сериализира в JSON и изпраща отговор.

**Авторизация**:  
Атрибутът `[Authorize]` на даден action означава: „позволи достъп само ако в заявката има валиден JWT“. Ако няма токен или е невалиден → 401 Unauthorized. Токенът **не** се чете в контролера ръчно – middleware-ът го валидира преди да стигне до action-а.

---

### 2.2. В Web MVC (Приложение 2)

**Роля**: няма собствена БД за дървета/таксономия. Контролерите **само** изпращат HTTP заявки към API и подават резултата към View.

| Файл | Роля |
|------|------|
| `Controllers/HomeController.cs` | Показва начална страница (само View). |
| `Controllers/AuthController.cs` | GET/POST за Login и Register. При POST вика `POST {ApiBaseUrl}/api/auth/login` или `.../register`, чете отговора и ако има `token`, записва го в **сесията** (`HttpContext.Session.SetString("Token", token)`). Logout изтрива токена от сесията. |
| `Controllers/TreesController.cs` | Index/Details/Create. Използва `ILeafMapApiClient` (обвивка над HttpClient): `_api.GetAsync<List<TreeVm>>("api/trees?includeLookups=true")`, `_api.PostAsync("api/trees", body)` и т.н. |
| `Controllers/TaxonomyController.cs` | Вика `_api.GetAsync(...)` за divisions, taxonomyclasses, genera, families, species и подава данните към View. |
| `Controllers/MapController.cs` | Вика API за списък дървета и ги подава към View. |

**Ключово**: в нито един от тези контролери **няма** `DbContext` или директна работа с БД. Всички данни идват от отговорите на API-то чрез `ILeafMapApiClient`.

**Как се подава JWT от Web към API**  
`Services/LeafMapApiClient.cs` при всяка заявка чете токена от сесията и го слага в заглавката:

```csharp
var token = _httpContext.HttpContext?.Session.GetString("Token");
if (!string.IsNullOrEmpty(token))
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
```

Така при POST/PUT/DELETE към API, заявките автоматично носят токена и API-то ги приема като автентикирани.

---

### 2.3. В Mobile MAUI (Приложение 3)

**Роля**: същата като Web – само HTTP клиент към API. Няма БД; всички данни от API.

| Файл | Роля |
|------|------|
| `Services/LeafMapApiService.cs` | Един централен клас за всички заявки: `GetTreesAsync()`, `GetTreeAsync(id)`, `LoginAsync()`, `RegisterAsync()`, `CreateTreeAsync()`, `GetDivisionsAsync()` и т.н. Използва един `HttpClient` с `BaseAddress = ApiSettings.BaseUrl`. |
| `Services/AuthService.cs` | Записва/чете/изтрива JWT чрез **SecureStorage** (платформено сигурно хранилище на телефона). |
| `Pages/*.xaml.cs` | Страниците викат `LeafMapApiService` и `AuthService`. Напр. `LoginPage` при успешен login вика `_auth.SetTokenAsync(resp.Token)` и после навигира към списъка с дървета. |

**Как се подава JWT от Mobile към API**  
В `LeafMapApiService` при всяка заявка се вика `EnsureTokenAsync()`, което взима токена от `AuthService` (SecureStorage) и го слага в заглавката:

```csharp
private async Task EnsureTokenAsync()
{
    var token = await _auth.GetTokenAsync();
    _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
        ? null
        : new AuthenticationHeaderValue("Bearer", token);
}
```

Така всички заявки от приложението (вкл. GET за списък и POST за ново дърво) носят един и същ токен, докато потребителят е „логнат“.

---

## 3. Как работи JWT в кода (от край до край)

### 3.1. Какво е JWT в този проект

- **JWT** е символен низ (токен), който API-то **издава** след успешен Login или Register.
- Токенът съдържа данни (напр. userId, email) и е **подписан** с таен ключ. Сървърът може да провери дали е издаден от него и дали не е подправен/изтекъл.
- Клиентът (Web или Mobile) **пази** този низ и го изпраща при всяка заявка в заглавката `Authorization: Bearer <токен>`.
- API-то **не пази** списък с активни сесии – само проверява подписа и срока на токена. Ако е валиден → счита заявката за автентикирана.

---

### 3.2. Къде се издава JWT (API)

**Файл**: `ASPLeadMapInsightsAPI/Services/JwtService.cs`

- Методът `GenerateToken(userId, email, roles)`:
  - Взима тайния ключ от конфигурацията (`Jwt:Key` в `appsettings.json`).
  - Създава списък от **claims** (напр. `ClaimTypes.NameIdentifier` = userId, `ClaimTypes.Email` = email).
  - Създава `JwtSecurityToken` с издател (`Issuer`), аудитория (`Audience`), време на изтичане (напр. 60 минути) и подпис с този ключ.
  - Връща низът на токена чрез `JwtSecurityTokenHandler().WriteToken(token)`.

**Файл**: `ASPLeadMapInsightsAPI/Controllers/AuthController.cs`

- **Register**: създава потребител с `UserManager.CreateAsync(user, password)`. При успех вика `_jwtService.GenerateToken(user.Id, user.Email!)` и връща в отговора обект с `token`, `email`, `expiresAt`.
- **Login**: проверява email/парола с `UserManager.FindByEmailAsync` и `CheckPasswordAsync`. При успех отново генерира токен чрез `_jwtService.GenerateToken(...)` и връща същия формат.

Така JWT **се създава само** в API, в `JwtService` + `AuthController`, и се връща в тялото на JSON отговора при login/register.

---

### 3.3. Къде се валидира JWT (API)

**Файл**: `ASPLeadMapInsightsAPI/Program.cs`

- Регистрира се схемата за автентикация и се конфигурира JWT Bearer:

```csharp
builder.Services.AddAuthentication(options => { ... })
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
```

- Стойностите `jwtKey`, `jwtIssuer`, `jwtAudience` идват от `appsettings.json` (секция `Jwt`).
- В pipeline-а се извикват `app.UseAuthentication()` и `app.UseAuthorization()`.

**Какво прави middleware-ът** при всяка заявка:

1. Ако в заявката има заглавка `Authorization: Bearer <токен>`, middleware-ът за JWT:
   - извлича низа на токена;
   - проверява подписа с `IssuerSigningKey`;
   - проверява издател и аудитория;
   - проверява дали токенът не е изтекъл (`ValidateLifetime`).
2. Ако всичко е наред → маркира заявката като „автентикирана“ и попълва `User` (claims) за текущия контекст. Контролерът може да използва `User` ако иска, но за самото „дали да влезе в action-а“ е достатъчно да има валиден токен.
3. Ако action-ът е с `[Authorize]` и токенът липсва или е невалиден → middleware-ът връща **401 Unauthorized** и контролерът изобщо не се изпълнява.

Така **валидирането** на JWT става изцяло в API-то, в конфигурацията на `AddJwtBearer` и в middleware-а за автентикация – не в отделен код в контролерите.

---

### 3.4. Къде се пази JWT в клиентите

| Клиент | Файл | Как се пази |
|--------|------|-------------|
| Web MVC | `AuthController.cs` | При успешен login/register: `HttpContext.Session.SetString("Token", token)`. При logout: `HttpContext.Session.Remove("Token")`. |
| Web MVC | `LeafMapApiClient.cs` | При всяка заявка: `var token = _httpContext.HttpContext?.Session.GetString("Token")` и слага го в `Authorization: Bearer ...`. |
| Mobile MAUI | `AuthService.cs` | `SecureStorage.Default.SetAsync("LeafMapJwtToken", token)` при login/register; `SecureStorage.Default.GetAsync("LeafMapJwtToken")` при четене; `SecureStorage.Default.Remove("LeafMapJwtToken")` при изход. |
| Mobile MAUI | `LeafMapApiService.cs` | Преди всяка заявка вика `EnsureTokenAsync()`, който взима токена от `AuthService` и го задава на `_http.DefaultRequestHeaders.Authorization`. |

Нито Web, нито Mobile **интерпретират** съдържанието на JWT – само го съхраняват и го изпращат обратно към API. Декодирането и проверката са само на сървъра.

---

### 3.5. Обобщен поток за един „логнат“ потребител

1. Потребител въвежда email/парола в Web или Mobile.
2. Клиентът изпраща `POST /api/auth/login` с JSON `{ "email": "...", "password": "..." }`.
3. API (`AuthController`) проверява с Identity и при успех вика `JwtService.GenerateToken(...)` и връща `{ "token": "...", "email": "...", "expiresAt": "..." }`.
4. Web записва `token` в сесията; Mobile записва в SecureStorage.
5. При следваща заявка (напр. `POST /api/trees`) клиентът добавя заглавка `Authorization: Bearer <token>`.
6. В API middleware-ът за JWT валидира токена; ако е валиден, заявката стига до контролера и `[Authorize]` е удовлетворен.
7. След изтичане на срока на токена (напр. 60 мин) валидацията връща грешка и клиентът трябва отново да викне login, за да получи нов токен.

---

## 4. Кратко указание по файлове

| Какво търсиш | Къде е |
|--------------|--------|
| Издаване на JWT (claims, подпис, expiry) | `ASPLeadMapInsightsAPI/Services/JwtService.cs` |
| Login/Register endpoints и връщане на token | `ASPLeadMapInsightsAPI/Controllers/AuthController.cs` |
| Конфигурация за валидиране на JWT (ключ, issuer, audience) | `ASPLeadMapInsightsAPI/Program.cs` (AddJwtBearer) и `appsettings.json` (секция Jwt) |
| Защита на write операции (изискване за токен) | `[Authorize]` върху POST/PUT/DELETE в контролерите в API |
| Запазване на токена в Web | `ASPLeafMapInsightsWebMVC/Controllers/AuthController.cs` (Session); използване при заявки: `Services/LeafMapApiClient.cs` |
| Запазване на токена в Mobile | `MAUILeafMapInsights/Services/AuthService.cs` (SecureStorage); използване при заявки: `Services/LeafMapApiService.cs` (EnsureTokenAsync) |
| CRUD в API (как се чете/пише в БД) | Всички `Controllers/*.cs` в API + `Data/LeafMapDbContext.cs` |
| CRUD от Web (без БД, само HTTP) | `ASPLeafMapInsightsWebMVC/Controllers/TreesController.cs`, `TaxonomyController.cs` и др. + `Services/LeafMapApiClient.cs` |
| Извиквания към API от Mobile | `MAUILeafMapInsights/Services/LeafMapApiService.cs` и `Pages/*.xaml.cs` |

С тази документация може да проследиш как се свързват приложенията, как работят контролерите и как точно JWT се издава, пази и валидира в кода. Когато дооправяш нещо, можеш да допълваш този файл с конкретни промени.
