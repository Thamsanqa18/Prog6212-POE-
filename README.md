# RaceDay — Part 3 MVC Frontend

RaceDay is a .NET 8 MVC web frontend integrated with the Part 2 ASP.NET Core Web API. The MVC application is a presentation layer only: it sends all reads, writes, authentication and image uploads to the API. It contains no EF Core reference, database connection, SQL queries or duplicated event/enrolment rules.

## Solution structure

- `RaceDay.Api/` — Part 2 REST API, EF Core data access, role enforcement and Azure Blob Storage endpoints.
- `RaceDay.Mvc/` — Part 3 MVC application, Razor views, session-aware navigation and API client.
- `RaceDay.Tests/` — Part 2 API integration tests.
- `docker-compose.yml` — SQL Server, API and MVC services.

## Part 1 / Part 2 alignment

The API data model follows the Part 1 relationships: a user owns an organiser or participant account; an organiser owns events; events have categories and enrolments; a participant can enrol once per event; and each enrolment can have one result.

The Part 1 endpoint document and the supplied Part 2 code use some different route names and request shapes. This MVC frontend deliberately consumes the routes and DTOs that are actually implemented in the supplied Part 2 API, including `/api/profile`, `/api/enrolments/mine`, `/api/events/{id}/categories` and `/api/results/mine`. It does not call the older `/api/users/me` or `/api/enrolments` list route described in the Part 1 endpoint table.

## Features

### Organiser
- Role-aware dashboard with event totals, enrolment counts and upcoming event count.
- Create, edit and delete running, walking and cycling events.
- Upload event banner images.
- Add and remove age or distance categories.
- View event enrolments and selected categories.
- Capture participant finish times and positions after the event.

### Participant
- Browse upcoming events and filter by Run, Walk or Cycle.
- View event details and available categories.
- Enrol in an event by selecting a category.
- View personal enrolments.
- View race history with finish time and ordinal position out of the total finishers.
- View and update profile information and upload a profile picture.

### Authentication and media
- Registration and login are performed through the Part 2 API.
- The MVC stores the API-issued HttpOnly authentication cookie in server-side session and keeps the user's role and basic display information in that session.
- The API remains responsible for authorization. The MVC sends the API cookie with every protected API request.
- All image uploads go from MVC to the API. Only the API writes to Azure Blob Storage.

## Run locally

Prerequisites: .NET 8 SDK and SQL Server / LocalDB.

1. Open `RaceDay.sln` in Visual Studio 2022.
2. Set `RaceDay.Api` as the startup project and configure `RaceDay.Api/appsettings.json`:
   - `ConnectionStrings:DefaultConnection`
   - `Registration:OrganiserCode`
   - `AzureBlobStorage:ConnectionString`
   - `AzureBlobStorage:ContainerName` (optional; defaults to `raceday-media`)
3. Start the API on port 5080:
   ```bash
   cd RaceDay.Api
   dotnet run --urls http://localhost:5080
   ```
4. In another terminal, start the MVC app:
   ```bash
   cd RaceDay.Mvc
   dotnet run
   ```
5. Browse to the MVC URL shown by `dotnet run`. The MVC API base URL defaults to `http://localhost:5080/`; change `Api:BaseUrl` in `RaceDay.Mvc/appsettings.json` if the API uses another port.

### Existing Part 2 database

The supplied Part 2 project uses `EnsureCreated` when no EF migrations exist. If your Part 2 database was already created before Part 3, add the two nullable media URL columns before running the updated API:

```sql
ALTER TABLE Events ADD BannerImageUrl nvarchar(max) NULL;
ALTER TABLE Users ADD ProfilePictureUrl nvarchar(max) NULL;
```

For a fresh database, the API's `EnsureCreated` creates the updated schema automatically. If you use EF migrations in your environment, create and apply a migration for these two properties instead of running the SQL above.

## Organiser registration

Participant registration is open. Organiser registration requires the organiser access code configured in the API's `Registration:OrganiserCode` setting. The default development code in the supplied Part 2 API is `RACEDAY-ORGANISER-2026`. Change it before deployment.

## Azure Blob Storage setup

Create an Azure Storage account and provide its connection string to the API as `AzureBlobStorage:ConnectionString`. The API creates the configured container (default `raceday-media`) on first upload. It stores images under `event-banners/` and `profile-pictures/`. The API accepts JPG, PNG and WebP images up to 5 MB and returns a read URL to the MVC. For a connection string containing an account key, the API creates a read-only SAS URL. Keep the storage connection string in environment variables or a secret store; do not commit it.

If Azure Blob Storage is not configured, the application still runs, but image upload requests return a clear configuration error.

## Docker

Docker Desktop is required. From the solution root:

1. Optionally set secrets:
   ```bash
   export MSSQL_SA_PASSWORD='Use-A-Strong-Password-Here!'
   export ORGANISER_CODE='your-private-organiser-code'
   export AZURE_BLOB_CONNECTION_STRING='your-azure-storage-connection-string'
   ```
2. Build and start SQL Server, the API and MVC frontend:
   ```bash
   docker compose up --build
   ```
3. Open **http://localhost:8081**. The API Swagger page is available at **http://localhost:5080/swagger**.

The single-command MVC-only alternative, when the API is already running and reachable from the container, is:

```bash
docker build -t raced ay-mvc ./RaceDay.Mvc
docker run --rm -p 8081:8080 -e Api__BaseUrl=http://host.docker.internal:5080/ raced ay-mvc
```

Use the compose workflow for the complete stack; it provides an internal `http://api:8080/` address for MVC and a SQL Server service for the API.

## Docker image security and configuration

- Multi-stage .NET 8 build/runtime images.
- Runs as the non-root `$APP_UID` runtime user.
- Configuration and secrets are supplied through environment variables.
- The MVC container has no database connection or Azure Storage credentials.

## Important deployment notes

- Replace development passwords and organiser codes before deployment.
- Use HTTPS and secure cookie settings behind a properly configured reverse proxy.
- Configure persistent SQL Server storage and Azure Storage lifecycle/backup policies.
- The current API's event result model stores finish time and position. Participant results show the event's recorded finisher count as the denominator.
