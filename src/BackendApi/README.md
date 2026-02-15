# API Internal Documentation

## Commands

* Create Migration
```shell
dotnet ef migrations add InitialMigration \
  --project PadTime.Infrastructure/PadTime.Infrastructure.csproj \
  --startup-project PadTime.API/PadTime.API.csproj
```
* Update Database
```shell
dotnet ef database update \
  --project PadTime.Infrastructure/PadTime.Infrastructure.csproj \
  --startup-project PadTime.API/PadTime.API.csproj
```

* Drop Database
```shell
dotnet ef database drop \
  --project PadTime.Infrastructure/PadTime.Infrastructure.csproj \
  --startup-project PadTime.API/PadTime.API.csproj \
  --force
```

* Remove Last Migration
```shell
dotnet ef migrations remove \
  --project PadTime.Infrastructure/PadTime.Infrastructure.csproj \
  --startup-project PadTime.API/PadTime.API.csproj
```

* Full Reset (Drop + Migrate)
```shell
dotnet ef database drop --force \
  --project PadTime.Infrastructure/PadTime.Infrastructure.csproj \
  --startup-project PadTime.API/PadTime.API.csproj && \
dotnet ef database update \
  --project PadTime.Infrastructure/PadTime.Infrastructure.csproj \
  --startup-project PadTime.API/PadTime.API.csproj
```
