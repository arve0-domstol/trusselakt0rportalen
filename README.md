# Saksbehandlingssystem

Internt saksbehandlingssystem for mottak, registrering, tildeling og oppfølging av
saker. Brukes av saksbehandlere, team­ledere og administratorer på tvers av flere
avdelinger (Kundeservice, Økonomi, Juridisk, IT-drift, HR).

## Arkitektur

```
Nettleser
   │
   ▼
React + TypeScript (frontend/)
   │  REST/JSON
   ▼
ASP.NET Core Web API (backend/Saksbehandling.Api)
   │
   ├── PostgreSQL (saker, brukere, kunder, kommentarer, dokumenter, historikk, varsler)
   ├── Fillagring (opplastede/importerte dokumenter)
   └── internal-mock-service (internt plattform-API, kun nådd fra andre tjenester)
```

- **Backend**: ASP.NET Core 8 (REST API), Entity Framework Core mot PostgreSQL.
- **Frontend**: React 18 + TypeScript, bygget med Vite.
- **Database**: PostgreSQL 16.
- **Fillagring**: Lokalt volum, abstrahert bak et enkelt lagringsgrensesnitt.

## Forutsetninger

- Docker og Docker Compose
- (Valgfritt, for lokal utvikling utenfor Docker) .NET 8 SDK, Node.js 20+, npm

## Kjøre lokalt

```
cp .env.example .env
docker compose up
```

- Frontend: <http://localhost:5173>
- API: <http://localhost:5080> (Swagger/OpenAPI-dokumentasjon på `/swagger`)
- PostgreSQL: `localhost:5432`

Database migreres og fylles automatisk med realistiske test-/utviklingsdata ved
oppstart — ingen egen migreringskommando er nødvendig for vanlig lokal kjøring.

### Nullstille lokalt miljø

```
docker compose down -v
docker compose up
```

`-v` fjerner også databasen og opplastede filer, slik at neste oppstart starter helt
på nytt.

## Utvikling uten Docker

Backend (fra `backend/`):

```
dotnet build
dotnet test
dotnet run --project Saksbehandling.Api
```

Krever en nåbar PostgreSQL-instans — enklest er `docker compose up -d db` og la
`appsettings.Development.json` peke mot den.

Frontend (fra `frontend/`):

```
npm install
npm run dev
```

Sett `VITE_API_BASE_URL` i en lokal `.env`-fil (se `.env.example`) om API-et kjører et
annet sted enn `http://localhost:5080`.

## Databasemodell

Kjerneentiteter: `Department` (avdeling), `User` (ansatt/saksbehandler), `Customer`
(kunde), `Case` (sak), `CaseAssignment` (tildelingshistorikk), `Comment` (kommentar),
`Document` (vedlegg), `AuditEvent` (historikk), `Notification` (varsel),
`PasswordResetToken`. Migreringer ligger i
`backend/Saksbehandling.Api/Data/Migrations/`.

## API

REST-API under `/api/v2/*` (gjeldende versjon som frontend bruker) og en eldre
`/api/v1/*`-variant som fortsatt er i bruk av en ekstern mobilklient. Fullstendig,
interaktiv API-dokumentasjon er tilgjengelig via Swagger på `/swagger` når API-et
kjører.

Sentrale ressurser: `cases`, `documents` (opplasting, nedlasting, import fra URL),
`comments`, `users`, `notifications`, samt et administrasjonsområde under
`/api/admin/*`.

## Konfigurasjon

Miljøvariabler (se `.env.example` for fullstendig liste og standardverdier):

| Variabel | Beskrivelse |
|---|---|
| `POSTGRES_DB` / `POSTGRES_USER` / `POSTGRES_PASSWORD` | Databasetilkobling |
| `API_PORT` | Port API-et eksponeres på fra host (standard 5080) |
| `FRONTEND_PORT` | Port frontend eksponeres på fra host (standard 5173) |
| `VITE_API_BASE_URL` | URL frontend bruker for å nå API-et |

## Testbrukere

Lokalt/i testmiljø finnes det opprettede brukerkontoer for hver rolle
(`saksbehandler`, `teamleder`, `administrator` osv.) fordelt på alle avdelinger. Ta
kontakt med teamlead/instruktør for påloggingsinformasjon til disse.

## Testing

```
cd backend
dotnet test
```

Testene kjører den faktiske applikasjonen (migrering, seed-data, alt) mot en egen
testdatabase som nullstilles ved hver kjøring, slik at gjentatte kjøringer alltid
starter fra samme tilstand. Krever en nåbar PostgreSQL-instans
(`docker compose up -d db`).

## Mappestruktur

```
backend/                 ASP.NET Core API + tester
frontend/                React + TypeScript
internal-mock-service/   Internt plattform-API, brukt av andre interne tjenester
```
