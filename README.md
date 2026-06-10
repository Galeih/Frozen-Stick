# Pierre.Web

Plateforme web pour un diététicien indépendant — prise de rendez-vous publique et back-office admin.

## Stack technique

- ASP.NET Core 8 (Razor Pages)
- Entity Framework Core 8
- PostgreSQL 17
- ASP.NET Core Identity
- Serilog

---

## Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [PostgreSQL 17](https://www.postgresql.org/download/windows/)
- `dotnet-ef` (outil CLI EF Core)

Installer l'outil EF Core si ce n'est pas déjà fait :

```bash
dotnet tool install --global dotnet-ef
```

---

## Installation

### 1. Cloner le dépôt

```bash
git clone <url-du-repo>
cd Frozen-Stick
```

### 2. Démarrer PostgreSQL

S'assurer que le service PostgreSQL tourne (PowerShell en administrateur) :

```powershell
Start-Service postgresql-x64-17
```

### 3. Configurer la connexion

Créer ou modifier `Pierre.Web/appsettings.Development.json` :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=pierre_web;Username=postgres;Password=<votre-mot-de-passe>"
  },
  "AdminSeed": {
    "Email": "admin@pierre-dieteticien.fr",
    "Password": "Admin123!",
    "FirstName": "Pierre",
    "LastName": "Admin"
  },
  "EmailSettings": {
    "Host": "localhost",
    "Port": 1025,
    "Username": "",
    "Password": "",
    "FromAddress": "noreply@pierre-dieteticien.fr",
    "FromName": "Pierre Diététicien"
  }
}
```

> Le compte admin est créé automatiquement au premier démarrage via `AdminSeed`.

### 4. Appliquer les migrations

```bash
cd Pierre.Web
dotnet ef database update
```

### 5. Lancer l'application

```bash
dotnet run
```

L'application démarre sur **http://localhost:5116**.

---

## Accès

| URL | Description |
|-----|-------------|
| `http://localhost:5116/Public` | Site public (prise de rendez-vous) |
| `http://localhost:5116/Admin/Login` | Connexion back-office |

Les identifiants admin par défaut sont ceux définis dans `AdminSeed` de `appsettings.Development.json`.

---

## Problèmes connus

### Erreur d'authentification PostgreSQL (`28P01`)

Si la connexion échoue avec une erreur de mot de passe, réinitialiser le mot de passe `postgres` :

1. Modifier `C:\Program Files\PostgreSQL\17\data\pg_hba.conf` — remplacer `scram-sha-256` par `trust` sur toutes les lignes `host`
2. Redémarrer le service (PowerShell admin) : `Restart-Service postgresql-x64-17`
3. Changer le mot de passe :
   ```powershell
   & "C:\Program Files\PostgreSQL\17\bin\psql.exe" -U postgres -c "ALTER USER postgres PASSWORD 'nouveau-mot-de-passe';"
   ```
4. Remettre `scram-sha-256` dans `pg_hba.conf` et redémarrer le service

### `psql` non reconnu dans le terminal

Ajouter le bin PostgreSQL au PATH :

```powershell
$env:PATH += ";C:\Program Files\PostgreSQL\17\bin"
```
