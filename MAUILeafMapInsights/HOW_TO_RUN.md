# LeafMap Insights – Android (MAUI) приложение

## 1. API URL за емулатор/телефон

Приложението използва **два сървъра**: **Data API** (дървета, таксономия) и **Auth API** (вход, регистрация). По подразбиране:
- **BaseUrl** = https://localhost:7234 (Data API)
- **AuthApiBaseUrl** = https://localhost:7240 (Auth API)

На емулатор/телефон localhost не работи – задай адресите на хоста:

- **Android емулатор**: в **Services/ApiSettings.cs** задай например:
  - `BaseUrl = "http://10.0.2.2:5202"` (Data API)
  - `AuthApiBaseUrl = "http://10.0.2.2:5203"` (Auth API)
- **Реално устройство** в същата Wi‑Fi: използвай IP на компютъра, напр. **http://192.168.1.5:5202** и **http://192.168.1.5:5203**.

## 2. Стартиране на API-тата

Пусни и двата сървъра (Data и Auth API). За емулатор те трябва да слушат на всички интерфейси, напр.:

```bash
# Терминал 1 – Data API
cd "ASPLeadMapInsightsAPI\ASPLeadMapInsightsAPI"
dotnet run --urls "http://0.0.0.0:5202"

# Терминал 2 – Auth API
cd "LeafMapInsightsAuthAPI\LeafMapInsightsAuthAPI"
dotnet run --urls "http://0.0.0.0:5203"
```

От емулатора достъп: **http://10.0.2.2:5202** (Data) и **http://10.0.2.2:5203** (Auth).

## 3. Пусни Android приложението

- От Visual Studio / Rider: избери **Android Emulator** и Run (F5).
- Или от терминал:
  ```bash
  cd "MAUILeafMapInsights\MAUILeafMapInsights"
  dotnet build -f net9.0-android
  dotnet run -f net9.0-android
  ```

## 4. Какво има в приложението

- **Начало** – бутони за Дървета, Таксономия, Добави дърво, Вход.
- **Дървета** – списък от API (дърпане надолу за обновяване), tap за детайли.
- **Таксономия** – отдели, класове, семейства, родове, видове от API.
- **Вход** – Login/Register към API; JWT се пази в SecureStorage.
- **Добави дърво** – формуляр (изисква логнат потребител).

Преди да тестваш, увери се че **ApiSettings.BaseUrl** и **ApiSettings.AuthApiBaseUrl** съвпадат с адресите на Data и Auth API от емулатора/телефона.
