# Paddle Reservation Platform

Paddle court reservation system with centralized authentication.

## Architecture

```
paddle-reservation/
├── src/
│   ├── IdentityServer/     # Duende IdentityServer + ASP.NET Identity
│   ├── BackendApi/         # API REST
│   └── AngularApp/         # Angular web client
├── docker-compose.yml
├── .env
└── README.md
```

## Services

| Service | Port Local | Technologie |
|---------|------------|-------------|
| identity-db | 5433 | PostgreSQL 18 |
| identity-server | 5001 | .NET 10 + Duende |

## Quick Start

### Prerequisites
- Docker Desktop
- .NET 10 SDK

###  Launch the complete environment

```bash
# Configuration
cp .env.example .env 

# Running
docker-compose up --build

# Access
# Identity Server: http://localhost:5001
```

## Local Development

**Terminal 1** - PostgreSQL only:
```bash
docker-compose up identity-db
```

**Terminal 2** - IdentityServer locally:
```bash
cd src/IdentityServer
dotnet watch run
```

Create `src/IdentityServer/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=identity_db;Username=identity_user;Password=VotrePassword"
  }
}
```

## Databases

### Migrations

```bash
# Create
cd src/IdentityServer
dotnet ef migrations add InitialMigration

# Apply (local)
dotnet ef database update

# Apply (Docker)
docker exec -it paddle-identity-server dotnet ef database update
```

### Seed

```bash
# Local
dotnet run /seed

# Docker
docker exec -it paddle-identity-server dotnet IdentityServer.dll /seed
```

## Docker commands

```bash
# Start
docker-compose up                            # All services
docker-compose up identity-db                # Specific service
docker-compose up -d --build                 # Rebuild + detached mode

# Stop
docker-compose down                          # Stop 
docker-compose down -v                       # Stop and delete volumes

# Debug
docker-compose logs -f                       # Real-time logs
docker-compose logs -f identity-server       # Service specific real-time logs
docker ps                                    # List all containers
docker exec -it paddle-identity-server bash  # Shell into container
```

## Technical stack

- **Backend**: .NET 10, Duende IdentityServer 7.4.3, ASP.NET Core Identity, EF Core 10
- **Database**: PostgreSQL 18
- **Frontend**: Angular v21, angular-auth-oidc-client

## Troubleshooting

**Database connexion error**: Verify that `identity-db` is healthy with `docker ps`

**Port already in use**: Change the ports in `docker-compose.yml`

**Container does not start**: `docker-compose logs -f <service-name>`
