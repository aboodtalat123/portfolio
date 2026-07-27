# ABD ALRAHMAN TALAT ABUSALEM Portfolio

A production-minded developer portfolio built with ASP.NET Core, Blazor Web App, EF Core, and SQLite.

## What is included

- VS Code Dark Modern and Light Modern visual themes with a persistent theme switcher.
- Static SSR landing page with selected work, reusable AI system, engineering lab, profile, professional journey, and contact sections.
- Full case-study routes for FairAid Gaza, the reusable site-aware AI layer, Dr. Solution, and Video Goals Extractor.
- Project content persisted through EF Core and exposed through read-only Minimal API endpoints.
- Responsive layouts for desktop, tablet, and mobile.
- CV download, social sharing image, favicon, error pages, SEO metadata, and automated repository tests.

## Architecture

- `Portfolio.Web`: public Static SSR pages, shared UI components, assets, and Minimal APIs.
- `Portfolio.Core`: domain entities, contracts, and presentation models.
- `Portfolio.Infrastructure`: EF Core persistence, database seeding, and repository implementations.
- `Portfolio.Tests`: automated persistence and project-route data tests.

## Open locally

Double-click `OPEN_PORTFOLIO.cmd`, or run:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/Portfolio.Web --no-launch-profile --urls "http://127.0.0.1:5180"
```

Then open `http://127.0.0.1:5180/`.

The development database is created at `src/Portfolio.Web/App_Data/portfolio.db` and updated idempotently with the current four project records.

## Read-only API

- `GET /api/health`
- `GET /api/projects?lang=en`
- `GET /api/projects?lang=ar`
- `GET /api/projects/{slug}?lang=en`

## Next content pass

- Replace temporary project captures with the final screenshot and video set after each product is demonstrated.
- Add confirmed live-demo links and LinkedIn only when the real URLs are supplied.
- Add Arabic case-study routes and a secured content administration area.
- Integrate the portfolio-facing version of the reusable site-aware AI assistant.
- Prepare the final Docker deployment after the content and domain are approved.
