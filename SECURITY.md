# Veiligheid en configuratie

Dit document beschrijft de configuratie en bekende aandachtspunten van dit onderwijsproject. Het is geen volledige beveiligingsaudit en geen garantie dat de applicatie geschikt is voor publiek productiegebruik.

## Veilig starten

Bij het opstarten voert `Program.cs` automatisch `Database.Migrate()` uit voor zowel `BordspelDbContext` als `AuthDbContext`. Controleer de verbindingen en bestaande migrations voordat je start. Gebruik een afzonderlijke ontwikkelomgeving en maak een back-up voordat je een bestaande database gebruikt.

De volledige applicatie is recent niet end-to-end met een actieve database getest. Controleer registratie, inloggen, het aanmaken en bekijken van avonden, deelname met en zonder snack en spellen toevoegen als organisator en als andere gebruiker. Controleer daarnaast HTTP-autorisatie, antiforgery, JWT-authenticatie en GraphQL.

## Lokaal configureren

Benodigd: de .NET 8 SDK (of een compatibele nieuwere SDK met .NET 8-runtime), Git en een bereikbare SQL Server. Configureer afzonderlijke databases voor applicatiegegevens en Identity. Voer onderstaande opdrachten uit vanuit de map met `BordspelAvond.sln`.

Het project heeft al een `UserSecretsId`. Stel de developmentconfiguratie in via .NET User Secrets; zet geen wachtwoorden of sleutels in bestanden die naar Git gaan. Vervang alle placeholders en gebruik de verbindingsopties die bij jouw SQL Server passen.

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=<SERVER>;Database=<BORDSPEL_DATABASE>;User Id=<GEBRUIKER>;Password=<WACHTWOORD>;Encrypt=True;TrustServerCertificate=False;" --project .\BordspelAvond.csproj
dotnet user-secrets set "ConnectionStrings:AuthConnection" "Server=<SERVER>;Database=<IDENTITY_DATABASE>;User Id=<GEBRUIKER>;Password=<WACHTWOORD>;Encrypt=True;TrustServerCertificate=False;" --project .\BordspelAvond.csproj
dotnet user-secrets set "Jwt:Key" "<CRYPTOGRAFISCH_WILLEKEURIGE_SLEUTEL_VAN_MINIMAAL_32_BYTES>" --project .\BordspelAvond.csproj
```

`Jwt:Issuer` staat in `appsettings.json`. User Secrets is bedoeld voor lokale ontwikkeling en is geen versleutelde productiekluis. Buiten Development gebruikt de applicatie `ConnectionStrings:BORDSPELDB_CONNSTR`, `ConnectionStrings:AUTHDB_CONNSTR` en `Jwt:Key`; configureer deze via de hostingomgeving, bijvoorbeeld met omgevingsvariabelen `ConnectionStrings__BORDSPELDB_CONNSTR`, `ConnectionStrings__AUTHDB_CONNSTR` en `Jwt__Key`.

## Historische credentials

De huidige configuratiebestanden bevatten geen databasewachtwoorden of JWT-sleutel. De bereikbare Git-geschiedenis bevat deze wel: databasewachtwoorden in `appsettings.Development.json` en een JWT-sleutel in `appsettings.json`, onder andere in commit `4f797f1`. Deze waarden staan ook in de geschiedenis op GitHub.

Of de historische credentials nog actief zijn, is niet vastgesteld. Er zijn geen aanmeldpogingen met deze waarden uitgevoerd. Beschouw gepubliceerde credentials als mogelijk gelekt:

1. Vervang of trek de wachtwoorden van de betrokken databaseaccounts en de JWT-sleutel in. Werk lokale User Secrets en eventuele hostingconfiguratie bij. JWT-rotatie maakt bestaande tokens ongeldig.
2. Schoon de geschiedenis gecontroleerd op met bijvoorbeeld `git-filter-repo`, na een beveiligde back-up en afstemming met eventuele medewerkers. Controleer alle relevante branches en tags opnieuw.
3. Stem publicatie van herschreven geschiedenis apart af. Oude clones, forks en caches kunnen de waarden blijven bevatten; opschonen vervangt credentialrotatie niet.

Een gewone commit die secrets verwijdert, verwijdert ze niet uit eerdere commits. Push geen ongecontroleerde oude geschiedenis opnieuw. Zet echte waarden of een vervangingsbestand voor history cleanup nooit in Git.

## Bestaande aandachtspunten

- De MVC-acties `Create` en `AddSnackToGameNight` missen nog antiforgery-validatie. `AddSnackToGameNight` heeft daarnaast geen eigen authenticatie- of eigendomscontrole.
- GraphQL Playground is ook buiten Development beschikbaar.
- De unit tests controleren bedrijfsregels, controllergedrag en beveiligingsattributen, maar bewijzen geen volledige bescherming van de HTTP-pipeline.

De recente verbeteringen controleren eigenaarschap bij spellen toevoegen via MVC en de API, beschermen de aangepaste MVC-acties met antiforgery en gebruiken POST voor deelname. Laat de resterende punten beoordelen voordat je de applicatie publiek host.
