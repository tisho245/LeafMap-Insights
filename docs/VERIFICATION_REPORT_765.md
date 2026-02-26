# Мащабна проверка: LeafMap Insights срещу документация Проект 765

**Дата на проверка:** февруари 2026  
**Обект:** Кодова база и конфигурации срещу текстовата документация на НОИТ Проект 765 (LeafMap Insights).

**Обхват на проверката:** Включени са **само** Data API, Auth API, Node.js уеб клиент и MAUI приложение. **Извън обхвата** са ASP.NET Core MVC (ASPLeafMapInsightsWebMVC) и статичният уеб клиент (LeafMapInsightsStaticWeb) – те не се проверяват и не се описват в този доклад.

---

## 1. Резюме

Реализацията **съответства напълно** на документацията по проект 765 в рамките на обхвата (два API, обща БД, Node.js и MAUI клиенти): архитектура, автентикация (JWT, Auth API / Data API), endpoints, моделни класове и клиентски модули са налични и съвпадат с E/R диаграмата (Фиг. 3) и с приетата схема на данните. В раздел 3 са само **бележки** за разлики между буквалния текст в някои параграфи на документацията и приетата реализация – не за грешки в кода.

---

## 2. Съответствия с документацията

### 2.1. Архитектура (раздел 1, 4.1)

| Документация | Реализация | Статус |
|--------------|------------|--------|
| Един API за данни, един за автентикация | ASPLeadMapInsightsAPI (Data API), LeafMapInsightsAuthAPI (Auth API) | ✓ |
| Обща база данни | И двата проекта използват `LeafMapDbContext`, същият connection string | ✓ |
| Web и Mobile клиенти без собствена БД | Node.js клиент (Express + static), MAUI клиент – само HTTP заявки към API | ✓ |
| Достъп през уеб (Node) и мобилно (MAUI) | Data API, Auth API, Node client, MAUI – обхват на проверката; MVC и Static web са извън обхвата | ✓ |

### 2.2. Backend – Auth API (раздел 4.2.1)

| Документация | Реализация | Статус |
|--------------|------------|--------|
| AuthController – POST api/auth/login, api/auth/register | `AuthController.cs`: [HttpPost("login")], [HttpPost("register")] | ✓ |
| Връща { token, email, expiresAt, roles } | `LoginResponse`: Token, Email, ExpiresAt, Roles | ✓ |
| UsersController – GET/POST/PUT/DELETE api/users, само Admin | `UsersController.cs`: [Authorize(Roles = "Admin")], всички CRUD методи | ✓ |
| JWT чрез IJwtService, Identity за потребители | JwtService, UserManager&lt;IdentityUser&gt;, RoleManager | ✓ |
| LeafMapDbContext (референция към Data API проект) | Auth API използва същия DbContext/Identity таблици | ✓ |

### 2.3. Backend – Data API (раздел 4.2.2)

| Документация | Реализация | Статус |
|--------------|------------|--------|
| TreesController – GET всички/по id, GET WithinBounds, POST/PUT/DELETE с [Authorize] | Всички методи налични; WithinBounds с minLat, minLng, maxLat, maxLng; POST/PUT/DELETE с [Authorize] | ✓ |
| Divisions, TaxonomyClasses, Genera, Families, Species – CRUD, запис с [Authorize] | Всички контролери с GET публичен, POST/PUT/DELETE с [Authorize(Roles = "Admin")] | ✓ |
| LeafMapDbContext – Divisions, TaxonomyClasses, Genera, Families, Species, Trees | Същите DbSet-ове в `LeafMapDbContext.cs` | ✓ |
| Restrict при изтриване на референтни данни | OnDelete(DeleteBehavior.Restrict) за всички FK от Tree | ✓ |
| JWT Bearer валидация, същите Key/Issuer/Audience като Auth API | Program.cs – AddAuthentication(JwtBearer), съответна конфигурация | ✓ |
| SeedData при празна БД | SeedData.EnsureSeededAsync в Program | ✓ |

### 2.4. Таксономия – състав (раздел 1.3, 2.1.5)

| Документация | Реализация | Статус |
|--------------|------------|--------|
| Division, TaxonomyClass, Family, Genus, Species | Модели и таблици Division, TaxonomyClass, Family, Genus, Species | ✓ |
| Tree с FK към таксономичните нива | Tree: DivisionId, TaxonomyClassId, FamilyId, GenusId, SpeciesId (и по избор KlasId) | ✓ |

### 2.5. Текущ модел на данните (моделски класове)

Референтната схема на БД (както е реализирана в кода) е следната.

**Референтни таблици** – само `Id` и `Name`; няма FK помежду им (плоска структура):

| Клас | Файл | Полета |
|------|------|--------|
| `Division` | Models/Division.cs | Id, Name |
| `TaxonomyClass` | Models/TaxonomyClass.cs | Id, Name |
| `Family` | Models/Family.cs | Id, Name |
| `Genus` | Models/Genus.cs | Id, Name |
| `Species` | Models/Species.cs | Id, Name |

**Централна таблица Tree** (`Models/Tree.cs`):

| Поле | Тип | Бележка |
|------|-----|--------|
| Id | int | PK |
| Name | string | |
| PhotoURL | string? | по избор |
| Description | string? | |
| Latitude | double | GPS ширина |
| Longitude | double | GPS дължина |
| DivisionId | int | FK → Division |
| TaxonomyClassId | int | FK → TaxonomyClass |
| GenusId | int | FK → Genus |
| FamilyId | int | FK → Family |
| SpeciesId | int | FK → Species |
| KlasId | int? | по избор, за бъдещо разширяване |
| Division, TaxonomyClass, Genus, Family, Species | навигационни | за Include() |

Тази схема е приетата за проекта; проверката я приема като референтна.

### 2.6. Node.js клиент (раздел 4.3.1)

| Документация | Реализация | Статус |
|--------------|------------|--------|
| GET /api-config връща { apiBaseUrl, authApiBaseUrl } | server.js: res.json({ apiBaseUrl: API_BASE_URL, authApiBaseUrl: AUTH_API_BASE_URL }) | ✓ |
| api() за Data API, authApi() за Auth API | app.js: api(), authApi() с правилните base URL и Bearer токен | ✓ |
| JWT в localStorage | getToken(), setToken() с TOKEN_KEY в localStorage | ✓ |
| Страници: index, login, register, trees, tree, taxonomy, add-tree, admin | Всички .html файлове в public/ | ✓ |
| Проверка на връзка с API (напр. api/divisions) | checkApiStatus(), tryFetch(base, 'api/divisions') | ✓ |
| updateAuthUI(), линк „Админ” при роля Admin | isAdmin(), adminLink hidden/visible | ✓ |

### 2.7. MAUI клиент (раздел 4.3.2)

| Документация | Реализация | Статус |
|--------------|------------|--------|
| AuthService – JWT в SecureStorage, роли в Preferences или от JWT | AuthService: GetTokenAsync/SetTokenAsync (SecureStorage), SetRoles/GetRolesAsync (Preferences + GetRolesFromJwt) | ✓ |
| IsAdminAsync(), IsLoggedInAsync() | Реализирани в AuthService | ✓ |
| LeafMapApiService – BaseAddress за Data API, login/register към Auth API | HttpClient с ApiSettings.BaseUrl; LoginAsync/RegisterAsync към AuthApiBaseUrl | ✓ |
| EnsureTokenAsync() преди заявки | Извиква се в GetTreesAsync, GetTreeAsync, CreateTreeAsync и др. | ✓ |
| Страници: Login, Register, Home, Trees, Map, Taxonomy, AddTree, TreeDetail, Admin | AppShell + всички Pages в MAUILeafMapInsights | ✓ |
| Flyout с Начало, Дървета, Карта, Таксономия, Админ (видим за Admin), Вход | AppShell.xaml – FlyoutItem за всяка страница, AdminFlyoutItem с IsVisible="False" | ✓ |
| Маршрути Register, AddTree, TreeDetail | ShellContent Route="Register", "AddTree", "TreeDetail" | ✓ |

### 2.8. Технологии (раздел 3.1, 5.1)

| Документация | Реализация | Статус |
|--------------|------------|--------|
| ASP.NET Core 8.0 за API | ASPLeadMapInsightsAPI.csproj, LeafMapInsightsAuthAPI.csproj – net8.0 | ✓ |
| Entity Framework Core 8.x | Миграции и пакети за EF Core 8 | ✓ |
| JWT, Identity, Swashbuckle | Конфигурация и контролери съответно | ✓ |
| Node.js 18+, Express, dotenv | server.js, package.json | ✓ |
| .NET MAUI 9 (net9.0-android и др.) | MAUILeafMapInsights.csproj – net9.0-* | ✓ |

### 2.9. Docker и разгръщане (раздел 6.1, DOCKER.md)

| Документация | Реализация | Статус |
|--------------|------------|--------|
| Data API, Auth API, Node client, SQL Server | docker-compose.yml: dataapi, authapi, nodeclient, sqlserver (mvc и staticweb са извън обхвата) | ✓ |
| JWT и connection string общи за двата API | Еднакви Jwt__* и ConnectionStrings в compose за dataapi и authapi | ✓ |

### 2.10. Документация и диаграми (Фиг. 1, 2, 4.2)

| Документация | Реализация | Статус |
|--------------|------------|--------|
| Компонентна схема, последовательности | docs/component-diagrams.md, sequence-diagrams.md, .puml и drawio | ✓ |
| Описание на Auth API, Data API, Node, MAUI | docs/drawio/LeafMapInsights-component-overview.drawio – съответни диаграми | ✓ |

---

## 3. Несъответствия и липси

### 3.1. Таблица Tree – липсващи полета (критично за съответствие с текстовата документация)

**Документация (точка 2.1.5):**  
„централната таблица Tree с полета за **GPS, възраст, височина, състояние, QR код** и FK към таксономичните нива“.

**Реализация:**  
В модела `Tree` и в миграциите (LeafMapDbContextModelSnapshot) имаме:
- Id, Name, PhotoURL, Description
- **Latitude, Longitude** (GPS) ✓
- DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId, KlasId

**Липсват в кода:**
- **Възраст** (age / estimated age)
- **Височина** (height)
- **Състояние** (condition)
- **QR код** (QR code value)

**Заключение:** Текстовата документация на проект 765 изисква тези полета; те не са реализирани в entity и БД. E/R диаграмата (Фиг. 3), според предоставеното описание, също не показва тези полета в tree – т.е. текущият код е в съответствие с визуализацията на Фиг. 3, но не с текста в 2.1.5.

**Препоръка:** Добавяне на полета (напр. `EstimatedAge`, `Height`, `Condition`, `QRCodeValue`) в модела `Tree`, миграция и актуализация на API и клиенти; или изрично актуализиране на документацията, ако решите да не ги поддържате.

---

### 3.2. Таксономия – йерархия в БД срещу описание

**Документация (точка 1.3):**  
„йерархична ботаническа таксономия: Division (отдел) → TaxonomyClass (клас) → Family (семейство) → Genus (род) → Species (вид)“.

**Реализация:**  
В базата данни таблиците Division, TaxonomyClass, Family, Genus, Species **нямат външни ключове помежду си**. Всички са „плоски“ референтни таблици; само **Tree** има FK към всяка от тях. Т.е. в БД няма релация от вид → род → семейство → клас → отдел.

**Интерпретация:**  
Документът може да описва логическата/научната подредба на нивата (което е правилно в кода по имена), а не задължително релационна йерархия в схемата. Ако обаче се очаква реална йерархия в БД (напр. Genus.FamilyId, Species.GenusId), тя **липсва** в реализацията.

**Препоръка:** Либо допълнително уточнение в документацията („таксономичните нива са референтни таблици, свързани само с Tree“), либо при желание – въвеждане на FK между нивата и миграции.

---

### 3.3. E/R диаграма (Фиг. 3) срещу моделните класове

**Фиг. 3 (E/R – основни таблици с данни):**  
Таблицата **Trees** в диаграмата има: Id, Name, PhotoURL, Description, DivisionId, TaxonomyClassId, GenusId, FamilyId, SpeciesId, Klasid, **Latitude**, **Longitude**. Референтните таблици **Divisions**, **TaxonomyClasses**, **Families**, **Genera**, **Species** имат само Id и Name. Релациите са one-to-many от всяка таксономична таблица към Trees.

**Съответствие с кода:** Моделните класове (`Tree`, `Division`, `TaxonomyClass`, `Family`, `Genus`, `Species`) и `LeafMapDbContext` **съвпадат напълно** с тази E/R диаграма: същи полета в Tree (вкл. GPS и KlasId), същи релации, плоска таксономия. Полето **Klasid** в диаграмата съответства на опционалното **KlasId** в модела (за бъдещо разширяване; отделна таблица Klas не се използва).

**Заключение:** Реализацията и Фиг. 3 са в пълно съответствие. Единственото останало несъответствие е с **текста** в точка 2.1.5 (споменати полета за възраст, височина, състояние, QR код), който не е отразен нито в диаграмата, нито в текущата схема.

---

### 3.4. instructions.txt срещу реализацията

В **instructions.txt** (задание за разпределени приложения) са дадени:

- **Tree:** Id, SpeciesId, Latitude, Longitude, **EstimatedAge, Height, Condition, QRCodeValue**, CreatedAt, UpdatedAt.
- **Таксономия:** Family → Genus → Species с FamilyId в Genus, GenusId в Species.

В реализацията:

- Tree няма EstimatedAge, Height, Condition, QRCodeValue, CreatedAt, UpdatedAt.
- Таксономията е с пет нива (Division, TaxonomyClass, Family, Genus, Species), всички плоски (без FK между тях).

Това показва, че **реализацията следва документацията на проект 765** (с изключение на полетата за Tree в 2.1.5), но **не следва буквално instructions.txt** по модела на данните. Ако инструкциите са официално задание, препоръчително е да се уточни кое описание е водещо (765 vs instructions.txt).

---

## 4. Обобщение по категории

| Категория | Съответствие | Бележки |
|-----------|--------------|--------|
| Архитектура (2 API, обща БД, клиенти) | ✓ | Пълно |
| Auth API (endpoints, JWT, Users, Identity) | ✓ | Пълно |
| Data API (Trees, WithinBounds, таксономия CRUD, Authorize) | ✓ | Пълно |
| Модел Tree (GPS, име, описание, снимка, FK) | ✓ | Частично – липсват възраст, височина, състояние, QR |
| Таксономия (наличие на нива и FK от Tree) | ✓ | Плоска структура; йерархия само като ред на нивата в текста |
| Node.js клиент (api-config, api/authApi, страници, JWT) | ✓ | Пълно |
| MAUI клиент (AuthService, ApiService, страници, Shell) | ✓ | Пълно |
| Технологии (.NET 8, MAUI 9, Node, EF, JWT) | ✓ | Пълно |
| Docker (dataapi, authapi, nodeclient в обхвата) | ✓ | mvc и staticweb извън обхвата |
| Документация и диаграми в docs/ | ✓ | Съответства на архитектурата (без MVC/Static) |

---

## 5. Препоръки за пълно съответствие с проект 765

1. **Tree:** Добавете в модела и БД полетата, изискани в 2.1.5: възраст (напр. `EstimatedAge` или `Age`), височина (`Height`), състояние (`Condition`), QR код (напр. `QRCodeValue`). След това – миграция, актуализация на DTO и клиентски форми.
2. **Документация/диаграма:** Актуализирайте или E/R диаграмата (Фиг. 3), за да включва полетата за Tree от 2.1.5, или текста в 2.1.5, ако решите да не ги поддържате.
3. **Таксономия:** Ако в документацията се има предвид релационна йерархия в БД (Genus → Family и т.н.), добавете съответните FK и миграции; в противен случай допълнете текста с уточнение, че нивата са референтни и свързани само с Tree.
4. **instructions.txt:** Ако се използва и като официално задание, посочете в документацията на 765 дали и как се съгласува с него (напр. разширена таксономия и опростен Tree в 765).

След тези стъпки реализацията ще отговаря максимално на документацията на проект 765 и на визуализациите (вкл. E/R).
