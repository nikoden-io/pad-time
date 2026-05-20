<div align="center">

# 🎾 Pad'Time

**A modern padel court reservation platform — with AI-powered slot suggestions.**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-21-DD0031?logo=angular&logoColor=white)](https://angular.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-18-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://docs.docker.com/compose/)
[![Pad'AI](https://img.shields.io/badge/Pad'AI-Gemini-8B5CF6?logo=googlegemini&logoColor=white)](https://ai.google.dev/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](#license)
[![Version](https://img.shields.io/badge/version-v2.1.0-blue.svg)](#)

</div>

---

## ✨ Overview

**Pad'Time** is a full-stack reservation platform for padel clubs. Players book courts and join public matches, admins steer the club from a rich KPI dashboard, and **Pad'AI** — our Gemini-powered assistant — suggests the best time slots and surfaces booking trends.

It is built as a **modular .NET 10 monolith** with **Clean Architecture + CQRS** behind a **standalone Angular 21 SPA**, with centralized authentication handled by **Duende IdentityServer 7**.

## 🚀 Key features

### 🎾 Players
- 📅 **Book a court** — sites, courts, multi-step calendar with conflict prevention
- 👥 **Public & private matches** — create or join open matches, share with friends
- 🤖 **Pad'AI suggestions** — smart slot recommendations based on availability and patterns
- 💳 **Frictionless payment** — pay your share after booking, with a payment success overlay
- 🏆 **Match history** — see upcoming, past and cancelled bookings at a glance
- 🌍 **i18n** — FR / EN / NL / DE

### 🛡️ Admins
- 📊 **KPI dashboard** — revenue, occupancy, member activity in real time
- 📈 **AI trends panel** — booking trends visualized via Gemini
- 👤 **Member management** — categories, debts, activity history
- 🔔 **Operational alerts** — incomplete matches, automatic debt creation
- 💶 **Analytics & revenue** — turnover by site, court, period

### 🔒 Platform
- 🔐 **OAuth2 / OIDC** — Authorization Code + PKCE via Duende IdentityServer
- 🏥 **Health endpoints** — `/health` and `/ready` for orchestration
- 🚀 **CI/CD** — GitHub Actions pipeline (build, test, deploy)
- 📦 **Containerized** — `docker-compose up` and the whole platform is online

## 🏗️ Architecture

```text
pad-time/
├── src/
│   ├── IdentityServer/          🔐 Duende IdentityServer 7 + ASP.NET Identity
│   ├── BackendApi/              ⚙️ .NET 10 modular monolith
│   │   ├── PadTime.API/         ↳ ASP.NET Core minimal-API host
│   │   ├── PadTime.Application/ ↳ CQRS (MediatR), validators, behaviors
│   │   ├── PadTime.Domain/      ↳ Entities, value objects, domain rules
│   │   └── PadTime.Infrastructure/ ↳ EF Core, Gemini client, persistence
│   └── AngularApp/              🎨 Angular 21 + PrimeNG + Tailwind
├── infra/                       ☁️  Bicep + deployment scripts (Azure)
├── docs/                        📚 Architecture, API reference, user manual
├── .github/workflows/           🤖 CI / CD pipelines
└── docker-compose.yml           🐳 Local orchestration
```

### Domain model — Clean Architecture + CQRS

```
┌────────────────┐   commands/queries   ┌────────────────┐
│   Angular 21   │ ───────────────────▶ │  PadTime.API   │
│   (SPA, OIDC)  │ ◀─── DTOs (JSON) ──  │  (Minimal API) │
└────────────────┘                      └───────┬────────┘
                                                │ MediatR
                                                ▼
                              ┌──────────────────────────────┐
                              │      PadTime.Application     │
                              │  Handlers · Validators · DTOs│
                              └────────┬────────────┬────────┘
                                       │            │
                            ┌──────────▼──┐   ┌─────▼─────────────┐
                            │   Domain    │   │  Infrastructure   │
                            │   Entities  │   │  EF Core + Gemini │
                            └─────────────┘   └───────┬───────────┘
                                                      │
                                                ┌─────▼─────┐
                                                │ PostgreSQL│
                                                └───────────┘
```

## 🧰 Tech stack

| Layer            | Technology                                                                 |
|------------------|----------------------------------------------------------------------------|
| **Backend**      | .NET 10 · ASP.NET Core · MediatR 12 · FluentValidation · EF Core 10        |
| **Auth**         | Duende IdentityServer 7.4 · ASP.NET Core Identity · OIDC + PKCE            |
| **Frontend**     | Angular 21 (standalone) · PrimeNG 21 · Tailwind · angular-auth-oidc-client |
| **AI**           | Google Gemini API (slot suggestions, trend analysis)                       |
| **Database**     | PostgreSQL 18 (two schemas: identity + business)                           |
| **Testing**      | xUnit · FluentAssertions · NSubstitute (BE) · Cucumber + Playwright (FE)   |
| **Infra**        | Docker Compose · Azure Bicep · GitHub Actions                              |
| **i18n**         | `@jsverse/transloco` (FR, EN, NL, DE)                                      |

## 🐳 Quick start

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js ≥ 20](https://nodejs.org/) (only for local Angular dev)

### One-shot launch

```bash
# 1. Copy environment variables
cp .env.example .env

# 2. Lift the whole stack
docker-compose up --build
```

| Service         | URL                          | Description                  |
|-----------------|------------------------------|------------------------------|
| Angular SPA     | http://localhost:4200        | Web client                   |
| Backend API     | http://localhost:5002        | REST API + Swagger           |
| Identity Server | http://localhost:5001        | OIDC authority               |
| identity-db     | `localhost:5433`             | PostgreSQL (users)           |
| api-db          | `localhost:5434`             | PostgreSQL (business)        |

## 💻 Local development

### Backend

```bash
cd src/BackendApi
dotnet restore
dotnet run --project src/PadTime.API
```

### Frontend

```bash
cd src/AngularApp
npm install
npm start            # http://localhost:4200
```

### Identity Server

```bash
cd src/IdentityServer
dotnet watch run
```

## 🧪 Testing

```bash
# Backend — unit tests (Domain + Application + behaviors)
cd src/BackendApi
dotnet test

# Frontend — unit tests
cd src/AngularApp
npm test

# Frontend — E2E (Cucumber + Playwright)
npm run e2e
```

The backend suite covers the **CQRS handlers**, **validators**, **domain entities**, and **pipeline behaviors** (logging + validation). See `src/BackendApi/tests/PadTime.Tests/` for the full layout.

## 🗄️ Database & migrations

```bash
# Apply migrations locally
cd src/BackendApi/src/PadTime.API
dotnet ef database update

# Seed demo data (members, sites, courts, sample bookings)
dotnet run -- --seed
```

The `DemoSeeder` generates a complete club state — members across categories, sites in Brussels, courts, future and past bookings — perfect to demo the admin dashboard.

## 🤖 Pad'AI configuration

Pad'AI relies on the Google Gemini API. Provide a key via environment variable:

```bash
# .env
GEMINI_API_KEY=your_gemini_api_key_here
GEMINI_MODEL=gemini-2.0-flash
```

If the key is missing, the app falls back gracefully — booking still works, AI panels show a friendly "AI offline" notice.

## 📚 Documentation

The `docs/` folder contains the full bachelor project documentation:

- 📘 [API Reference](docs/API-Reference.md)
- 📗 [User Manual](docs/Manuel%20d'utilisation.md)
- 📐 P0 — Project Charter
- 🧭 P1 — Compréhension métier formalisée
- 🏛️ P3 — Architecture cible
- 🔒 P4 — Security model
- 📊 P5 — Stratégie Data & Analytics
- ✅ P7 — Qualité, CI/CD et critères de livraison

## 🛠️ Useful Docker commands

```bash
docker-compose up -d --build              # Rebuild + detached
docker-compose down -v                    # Stop + drop volumes
docker-compose logs -f api                # Tail API logs
docker exec -it pad-time-api bash         # Shell into the API container
```

## 🚑 Troubleshooting

| Symptom                              | Fix                                                              |
|--------------------------------------|------------------------------------------------------------------|
| `connection refused` on db           | Wait for the health check — `docker ps` should show `(healthy)`  |
| Port already in use                  | Edit the host-side port in `docker-compose.yml`                  |
| Angular shows `401` on every call    | Re-login — your OIDC session has expired or cookies were cleared |
| AI panel shows "AI offline"          | Set `GEMINI_API_KEY` in `.env` and restart the API               |

## 📝 Changelog

See [CHANGELOG.md](CHANGELOG.md) for the full release history.

## 👤 Author

**Nicolas Denoel** — Bachelor project, 2026.

## 📄 License

Released under the [MIT License](LICENSE).

---

<div align="center">
<sub>Built with ❤️ and a lot of ☕ — powered by <strong>Pad'AI</strong> 🤖</sub>
</div>
