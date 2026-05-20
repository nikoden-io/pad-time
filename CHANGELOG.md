# Changelog

All notable changes to **Pad'Time** are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project follows [Semantic Versioning](https://semver.org/).

---

## [2.1.0] — 2026-05-20

### Added
- Comprehensive unit test suite covering the Domain and Application layers (admin, billing, booking, sites, validators, MediatR pipeline behaviors).
- Polished public-facing README with a clear quick-start, architecture diagram, and badge row.
- `CHANGELOG.md` to track release history going forward.

### Changed
- `.gitignore` now excludes editor configs (`.vscode/`, `.idea/`), AI-tooling artifacts (`docs/generate_report.py`, generated `.docx` / `.pdf` reports, Office lock files), scratch captures (`debug-screenshot.png`, `frontend-structure.txt`), and the internal `scenarios.md`.
- Repository state cleaned up — untracked editor/IDE files and scratch assets removed from version control.
- Pinned `System.Security.Cryptography.Xml` 10.0.8 to silence NU1903 transitive advisory.

### Fixed
- Docker volume mount paths for both PostgreSQL services normalized to `/var/lib/postgresql` to avoid permission edge cases on Docker Desktop.

---

## [2.0.0] — 2026-05-19

### Added
- 🤖 **Pad'AI** — Gemini-powered smart slot suggestions in the booking flow.
- 📊 **Admin AI trends panel** — Gemini-generated trend analysis in the admin dashboard.
- 📈 **Admin dashboard** — KPI cards, occupancy chart, revenue analytics, alerts.
- 👤 **Member management** — admin page with categories, debts and activity.
- 💳 **Payment flow** — pay-share button, confetti success overlay, optimistic local status update.
- 🎉 **Booking success overlay** — padel-themed burst animation + ordered match list.
- 👥 **Private match support** in the booking flow.
- 🔗 **Join match page** for public matches.
- 🩺 **Health endpoints** (`/health`, `/ready`) + GitHub Actions CI pipeline.
- 💶 **Auto-debt** generation on incomplete matches.
- 🌍 Enriched demo seeding + admin i18n translations + test scenarios doc.
- 📚 Complete API reference and v1.1 user manual.
- 🏷️ Version footer with Pad'AI badge.

### Fixed
- Serialize member category and match status as strings in admin DTOs.
- Removed broken `GetDailyBookingStatsAsync` call in the AI trends handler.

---

## [1.0.0] — 2026-03-31

### Added
- Initial release: Angular 21 SPA, .NET 10 modular monolith, Duende IdentityServer 7, PostgreSQL 18.
- Court reservation core flow (sites → courts → calendar → confirmation).
- OIDC + PKCE authentication.
- Docker Compose orchestration for local development.
- Azure Bicep infrastructure scripts.

---

[2.1.0]: https://github.com/your-org/pad-time/releases/tag/v2.1.0
[2.0.0]: https://github.com/your-org/pad-time/releases/tag/v2.0.0
[1.0.0]: https://github.com/your-org/pad-time/releases/tag/v1.0.0
