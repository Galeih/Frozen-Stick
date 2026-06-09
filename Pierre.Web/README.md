# Pierre.Web

Plateforme web pour un diététicien indépendant.

## Stack technique

- ASP.NET Core 8 (Razor Pages)
- Entity Framework Core 8
- PostgreSQL
- ASP.NET Core Identity
- Serilog

## Prérequis

- .NET 8 SDK
- PostgreSQL

## Lancer le projet

1. Configurer la chaîne de connexion dans `appsettings.Development.json`.
2. Appliquer les migrations :

```
dotnet ef database update
```

3. Lancer l'application :

```
dotnet run
```

Le projet démarre sur `http://localhost:5000`.
