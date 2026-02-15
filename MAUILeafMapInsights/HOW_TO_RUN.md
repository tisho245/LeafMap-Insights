# LeafMap Insights – Android (MAUI) приложение

## 1. API URL за емулатор/телефон

По подразбиране приложението вика **https://localhost:7234**. На емулатор/телефон това не работи.

- **Android емулатор**: използвай адрес на хоста: **http://10.0.2.2:5202** (ако API-то ти е на `http://localhost:5202`). Промени в кода:
  - В **Services/ApiSettings.cs** задай:  
    `public static string BaseUrl { get; set; } = "http://10.0.2.2:5202";`
- **Реално устройство** в същата Wi‑Fi мрежа: сложи API URL на IP-то на компютъра, напр. **http://192.168.1.5:5202**.

## 2. Стартиране на API-то

Първо пусни API-то на сървъра (ASPLeadMapInsightsAPI). За емулатор трябва да слуша на всички интерфейси (не само localhost), напр.:

```bash
cd "ASPLeadMapInsightsAPI\ASPLeadMapInsightsAPI"
dotnet run --urls "http://0.0.0.0:5202"
```

Така от емулатора ще може да достъпиш с **http://10.0.2.2:5202**.

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

Преди да тестваш, увери се че **ApiSettings.BaseUrl** съвпада с адреса на API-то от емулатора/телефона.
