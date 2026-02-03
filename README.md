# BookTracker

A simple book tracking web app built with ASP.NET Core (.NET 8), EF Core and Identity.

## Tech Stack
- ASP.NET Core MVC (.NET 8)
- Entity Framework Core (SQL Server)
- ASP.NET Core Identity
- xUnit for tests

## Prerequisites
- .NET SDK 8
- SQL Server / LocalDB

## Setup (Development)
1. Restore packages:
   dotnet restore

2. Apply migrations:
   dotnet ef database update --project bookTracker/bookTracker.csproj

3. Run:
   dotnet run --project bookTracker/bookTracker.csproj

## Configuration
- Connection string: `ConnectionStrings:DefaultConnection`
- Optional admin seeding:
  - Set `SeedAdmin__Password` as an environment variable (recommended)
  - Or use a local config file like `appsettings.Local.json` (ignored by git)

## Tests
dotnet test
