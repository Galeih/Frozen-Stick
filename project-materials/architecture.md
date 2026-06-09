# Architecture — Projet Pierre (Diététicien)

## Stack technique

| Couche | Technologie |
|---|---|
| Backend | ASP.NET Core 8 (LTS) |
| ORM | Entity Framework Core 8 |
| Base de données | PostgreSQL (production) / SQL Server LocalDB (développement) |
| Authentification | ASP.NET Core Identity |
| Rendu HTML | Razor Pages |
| Frontend | HTML / CSS / JavaScript vanilla (pas de framework JS lourd) |
| Email | SMTP configurable ou service tiers (MailKit) |
| Stockage fichiers | Dossier local (développement) / Azure Blob ou S3-compatible (production) |
| Import Excel | EPPlus ou ClosedXML |
| Logs | Serilog |

---

## Structure du projet

```
Pierre.Web/                        ← Projet principal ASP.NET Core
├── Pages/                         ← Razor Pages
│   ├── Public/                    ← Pages publiques (non authentifiées)
│   │   ├── Index.cshtml
│   │   ├── Presentation.cshtml
│   │   ├── Services.cshtml
│   │   ├── Contact.cshtml
│   │   ├── Contents/
│   │   │   ├── Index.cshtml
│   │   │   └── Detail.cshtml
│   │   └── Booking/
│   │       └── Request.cshtml
│   └── Admin/                     ← Pages back-office (authentifiées)
│       ├── Dashboard.cshtml
│       ├── Clients/
│       ├── Appointments/
│       ├── Contents/
│       ├── Planning/
│       ├── AdminTracking/
│       └── Import/
├── Application/                   ← Services applicatifs, use cases, DTOs
│   ├── Services/
│   ├── DTOs/
│   └── Interfaces/
├── Domain/                        ← Entités métier, règles, énumérations
│   ├── Entities/
│   ├── Enums/
│   └── Exceptions/
├── Infrastructure/                ← Accès données, email, fichiers, import
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── Migrations/
│   │   └── Repositories/
│   ├── Email/
│   ├── Storage/
│   └── Import/
└── wwwroot/                       ← Fichiers statiques (CSS, JS, images)
```

---

## Architecture en couches

```
[Web - Razor Pages]
    ↓ appelle
[Application - Services / Use Cases]
    ↓ utilise
[Domain - Entités / Règles métier]
    ↑ implémenté par
[Infrastructure - EF Core / Email / Fichiers]
```

- La couche **Web** ne contient que les contrôleurs de pages et la présentation.
- La couche **Application** orchestre les cas d'usage et ne dépend pas de l'infrastructure.
- La couche **Domain** contient les entités et règles pures, sans dépendance externe.
- La couche **Infrastructure** implémente les interfaces définies dans Application.

---

## Entités principales (Domain)

### User
```
Id           : Guid
Email        : string
PasswordHash : string          ← géré par ASP.NET Core Identity
CreatedAt    : DateTime
```

### Client
```
Id           : Guid
FirstName    : string
LastName     : string
Email        : string?
Phone        : string?
BirthDate    : DateOnly?
Notes        : string?         ← notes libres sur le client
IsArchived   : bool
CreatedAt    : DateTime
UpdatedAt    : DateTime
```

### Appointment
```
Id              : Guid
ClientId        : Guid?        ← nullable (pas encore associé)
SlotId          : Guid
RequesterName   : string       ← nom saisi dans le formulaire public
RequesterEmail  : string?
RequesterPhone  : string?
Message         : string?
Status          : AppointmentStatus (Pending / Accepted / Refused / Cancelled)
CreatedAt       : DateTime
UpdatedAt       : DateTime
```

### Availability (créneaux)
```
Id          : Guid
Date        : DateOnly
StartTime   : TimeOnly
EndTime     : TimeOnly
IsBlocked   : bool
```

### ConsultationNote
```
Id            : Guid
ClientId      : Guid
AppointmentId : Guid?
Date          : DateTime
Content       : string
Recommendations : string?
Weight        : decimal?
CreatedAt     : DateTime
UpdatedAt     : DateTime
```

### ContentPost
```
Id          : Guid
Title       : string
Slug        : string           ← généré depuis le titre
Type        : ContentType (Recipe / Article / News / Workshop / Tip)
Status      : ContentStatus (Draft / Published)
Body        : string           ← HTML ou Markdown
ImagePath   : string?
PublishedAt : DateTime?
CreatedAt   : DateTime
UpdatedAt   : DateTime
```

### ContactRequest
```
Id        : Guid
Name      : string
Email     : string?
Phone     : string?
Message   : string?
CreatedAt : DateTime
IsRead    : bool
```

### Invoice
```
Id          : Guid
ClientId    : Guid
Reference   : string
Amount      : decimal
Status      : InvoiceStatus (Pending / Paid / Cancelled)
IssuedAt    : DateOnly
Notes       : string?
```

### ImportedFile
```
Id          : Guid
FileName    : string
ImportedAt  : DateTime
RowCount    : int
ErrorCount  : int
Status      : ImportStatus (Pending / Validated / Cancelled)
```

### NotificationLog
```
Id          : Guid
RecipientEmail : string
Subject     : string
SentAt      : DateTime?
Success     : bool
Error       : string?
```

---

## Énumérations

```csharp
enum AppointmentStatus { Pending, Accepted, Refused, Cancelled }
enum ContentType       { Recipe, Article, News, Workshop, Tip }
enum ContentStatus     { Draft, Published }
enum InvoiceStatus     { Pending, Paid, Cancelled }
enum ImportStatus      { Pending, Validated, Cancelled }
```

---

## Relations

```
Client           1 ──< Appointment        (un client → plusieurs rendez-vous)
Client           1 ──< ConsultationNote   (un client → plusieurs notes)
Client           1 ──< Invoice            (un client → plusieurs factures)
Appointment      1 ──< ConsultationNote   (un rendez-vous → 0 ou 1 note)
Availability     1 ──  Appointment        (un créneau → 0 ou 1 rendez-vous accepté)
```

---

## Authentification

- ASP.NET Core Identity avec un seul compte administrateur.
- Le compte est créé via un seeder au démarrage si aucun utilisateur n'existe.
- Toutes les pages sous `/Admin/` sont protégées avec `[Authorize]`.
- Les pages publiques sous `/Public/` n'ont aucune restriction.
- Session basée sur cookie chiffré.
- Mot de passe haché avec PBKDF2 (géré par Identity).

---

## Gestion des emails

- Interface `IEmailService` dans Application.
- Implémentation `SmtpEmailService` dans Infrastructure.
- Configuration via `appsettings.json` (hôte SMTP, port, identifiants).
- Templates emails dans `Infrastructure/Email/Templates/`.
- Événements déclencheurs :
  - nouvelle demande de rendez-vous → notification au professionnel
  - rendez-vous accepté → confirmation au demandeur
  - rendez-vous refusé → information au demandeur

---

## Stockage des fichiers

- Interface `IFileStorageService` dans Application.
- Implémentation `LocalFileStorageService` en développement.
- Les fichiers sont servis via un chemin relatif dans `wwwroot/uploads/`.
- En production, prévoir un service cloud (Azure Blob, Cloudflare R2, etc.).

---

## Journalisation

- Serilog configuré pour écrire dans la console et dans un fichier `logs/app-.log` (rotation quotidienne).
- Actions métier sensibles journalisées via le service applicatif :
  - connexion / déconnexion
  - création / modification / archivage d'un client
  - ajout / modification d'une note de consultation
  - acceptation / refus d'un rendez-vous
  - publication d'un contenu
  - import de fichier
- Les logs ne contiennent jamais de mot de passe ni de données médicales.

---

## Configuration par environnement

`appsettings.json` (valeurs par défaut)
`appsettings.Development.json` (base locale, email en console)
`appsettings.Production.json` (base PostgreSQL, SMTP réel, stockage cloud)

Variables sensibles (mots de passe, clés API) via les secrets utilisateur (`dotnet user-secrets`) en développement et variables d'environnement en production.

---

## Initialisation de la base de données

Au démarrage de l'application :
1. Appliquer les migrations en attente (`context.Database.MigrateAsync()`).
2. Si aucun utilisateur administrateur n'existe, créer le compte par défaut depuis la configuration.
3. Optionnellement, seeder des données de démonstration si l'environnement est `Development`.
