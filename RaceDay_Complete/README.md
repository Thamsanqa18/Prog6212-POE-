# RaceDay API (PROG6212 Part 2 starter implementation)

ASP.NET Core Web API for managing race events, categories, participant enrolments and results. Built for .NET 8 with Entity Framework Core, SQL Server, JWT bearer authentication and Swagger.

## Important before submission
- This project is a generated implementation based on the supplied endpoint plan and SQL script. Build and test it locally, compare it against the full assessment rubric, and make sure you can explain the code in your own video.
- The supplied SQL script uses sample users with fake password hashes. Do not use those sample password hashes to log in. Register fresh users through `/api/auth/register`.
- For a quick local development setup, the connection string uses SQL Server LocalDB. If LocalDB is not installed, change it to your SQL Server instance in `RaceDay.API/appsettings.json`.
- Change the development JWT key before any deployment. Do not commit production secrets to GitHub.
- Role is accepted during public registration because the supplied endpoint plan explicitly includes it. For a real production application, Organiser privileges should be assigned by an administrator, not chosen by a public user.

## Requirements
- .NET 8 SDK
- SQL Server LocalDB or another SQL Server instance
- Visual Studio 2022 with ASP.NET and web development workload

## Run in Visual Studio 2022
1. Open `RaceDay.sln`.
2. In Solution Explorer, right-click `RaceDay.API` and choose **Set as Startup Project**.
3. Select the `RaceDay.API` (not Docker) launch profile.
4. Restore NuGet packages, then build the solution.
5. Create the database with EF Core migrations using Package Manager Console:
   - Set Default project to `RaceDay.API`.
   - Run `Add-Migration InitialCreate`.
   - Run `Update-Database`.
   If migrations are unavailable, install the EF Core Tools package compatible with EF Core 8, then repeat.
6. Press Ctrl+F5. Swagger should open at the local `/swagger` address.

Alternative terminal commands from the folder containing `RaceDay.sln`:
```powershell
dotnet restore RaceDay.sln
dotnet build RaceDay.sln
dotnet tool install --global dotnet-ef --version 8.0.11
dotnet ef migrations add InitialCreate --project RaceDay.API
dotnet ef database update --project RaceDay.API
dotnet run --project RaceDay.API
```

## API routes
- `POST /api/auth/register` — register account
- `POST /api/auth/login` — login and receive JWT
- `GET, PUT /api/users/profile` — own profile
- `GET /api/categories` — public categories
- `POST, PUT, DELETE /api/categories` — Organiser-only category management
- `GET /api/events`, `GET /api/events/{id}` — public event browsing
- `POST, PUT, DELETE /api/events` — Organiser-only event management
- `POST /api/events/{id}/enrol` — Participant enrolment
- `GET /api/events/{id}/enrolments` — Organiser views event enrolments
- `GET /api/users/enrolments` — Participant's enrolments
- `DELETE /api/enrolments/{id}` — Participant cancels own enrolment
- `POST, PUT /api/enrolments/{id}/result` — Organiser captures/updates result
- `GET /api/events/{id}/results` — public event results
- `GET /api/users/results` — Participant's own results

## Test
Run `dotnet test RaceDay.sln`. The unit tests cover role validation and event-capacity rules. Test the full API manually in Swagger too, including invalid credentials and role restrictions.

## Suggested Swagger demonstration
1. Register a test Organiser, log in and copy the JWT.
2. Click **Authorize** and enter the token as `Bearer YOUR_TOKEN` (Swagger's bearer scheme may ask for the token only).
3. Create a category and an event.
4. Register a Participant in a separate request and log in as that user.
5. Authorize with the Participant token and enrol in the event.
6. Switch back to the Organiser token and capture a result.
7. Verify public event results and the Participant's own enrolments/results.
8. Show the test results and GitHub Actions build.

## Design notes
The database has both the `Events.CategoryId` relationship from the SQL script and the `Event_Categories` join table. The API currently creates a primary category and a matching join-table row to preserve the supplied design. The ERD and SQL script should be compared and reconciled if the lecturer expects the ERD to be the final authority.
