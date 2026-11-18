# BordspelAvond

BordspelAvond is een zelfstandig ontwikkelde ASP.NET Core-webapplicatie voor het organiseren van en deelnemen aan bordspelavonden. Het project is een individuele onderwijsopdracht; de broncode en unit tests zijn beschikbaar als voorbeeld van mijn C#-programmeervaardigheden.

## Functionaliteiten

- Registreren en inloggen met ASP.NET Core Identity.
- Bordspelavonden aanmaken en bekijken, met locatie, datum, capaciteit en deelnemers.
- Overzichten van eigen georganiseerde avonden en deelnames.
- Deelnemen met controles op capaciteit, leeftijd en dieetvoorkeuren, plus een afzonderlijke deelnameflow met snack.
- Als organisator spellen toevoegen en avonden zonder deelnemers bewerken of verwijderen.
- Speldetails en snacks bekijken; HTTP-API en GraphQL-queries voor bordspelavonden.

## Techniek en architectuur

**C# · .NET 8 · ASP.NET Core MVC · Razor · Entity Framework Core · SQL Server · LINQ · ASP.NET Core Identity · JWT · GraphQL.NET · Swagger/OpenAPI · xUnit · Moq**

Het project past principes van Onion Architecture toe binnen één webproject, `BordspelAvond.csproj`. Services bevatten bedrijfsregels en gebruiken een repository-interface voor datatoegang. De EF Core-repository gebruikt LINQ voor queries; `Program.cs` koppelt interfaces en implementaties via Dependency Injection.

| Map | Verantwoordelijkheid |
| --- | --- |
| `Core/Domain`, `Core/Interfaces` | Domeinmodellen en repositorycontract |
| `Application/Interfaces`, `Application/Services` | Servicecontract en bedrijfsregels |
| `Infrastructure/Data`, `Infrastructure/Repositories` | Twee DbContexts, EF Core-repository en migrations |
| `Presentation` | MVC-controllers, API-controllers en GraphQL |
| `Views`, `Areas/Identity`, `wwwroot` | Razor-weergaven, Identity-pagina's en statische bestanden |
| `Tests` | Unit tests voor services en controllerbeveiliging |

De lagen zijn niet overal strikt gescheiden: sommige controllers gebruiken rechtstreeks een DbContext en Identity-types komen in meerdere lagen voor.

## Bouwen en testen

Benodigd: de .NET 8 SDK (of een compatibele nieuwere SDK met .NET 8-runtime). Voer vanuit de map met `BordspelAvond.sln` uit:

```powershell
dotnet restore
dotnet build
dotnet test
```

De tests gebruiken xUnit en Moq om bedrijfsregels, eigendomscontrole en deelname-afhandeling te controleren. De laatste controle gaf 42 geslaagde unit tests. De volledige applicatie is recent **niet end-to-end met een actieve database getest**.

## Lokaal starten

Configureer een bereikbare SQL Server en beide databaseverbindingen plus de JWT-sleutel via .NET User Secrets. Zie [SECURITY.md](SECURITY.md) voor de exacte configuratiecommando's, placeholders en aandachtspunten voor veilig gebruik.

**Bij opstarten worden automatisch migrations op beide databases toegepast.** Controleer eerst de configuratie en migrations; gebruik een afzonderlijke ontwikkelomgeving en maak een back-up voordat je een bestaande database gebruikt.

```powershell
dotnet dev-certs https --trust
dotnet run --project .\BordspelAvond.csproj --launch-profile https
```

Het HTTPS-profiel gebruikt Development op `https://localhost:7088`. Swagger is dan beschikbaar op `/swagger`; GraphQL gebruikt `/graphql`.
