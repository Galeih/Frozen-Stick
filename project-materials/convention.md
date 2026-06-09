# Conventions — Projet Pierre (Diététicien)

## Langue

- **Code** : anglais (noms de classes, méthodes, variables, propriétés, fichiers).
- **Commentaires** : français.
- **Interfaces utilisateur** : français.
- **Migrations EF Core** : anglais (ex: `AddClientTable`, `AddAppointmentStatusColumn`).

---

## Nommage C#

### Classes et interfaces
```
PascalCase
ClientService, AppointmentRepository, IEmailService, ContentPost
```

### Méthodes
```
PascalCase, verbe + complément
GetClientByIdAsync, CreateAppointmentAsync, PublishContentAsync
```

### Propriétés
```
PascalCase
FirstName, IsArchived, PublishedAt
```

### Variables locales et paramètres
```
camelCase
clientId, appointmentStatus, pageNumber
```

### Interfaces
```
Préfixe I
IClientRepository, IEmailService, IFileStorageService
```

### DTOs
```
Suffixe Dto
CreateClientDto, AppointmentListItemDto, ContentDetailDto
```

### ViewModels (Razor Pages)
```
Suffixe ViewModel ou Model (via le PageModel de Razor Pages)
ClientEditViewModel, AppointmentRequestModel
```

### Énumérations
```
PascalCase pour le type, PascalCase pour les valeurs
ContentStatus.Draft, AppointmentStatus.Accepted
```

### Constantes
```
PascalCase dans une classe statique
AppConstants.DefaultPageSize
```

---

## Nommage des fichiers

| Type | Convention | Exemple |
|---|---|---|
| Razor Page | PascalCase | `ClientEdit.cshtml` |
| PageModel | PascalCase + `.cshtml.cs` | `ClientEdit.cshtml.cs` |
| Service | PascalCase + `Service` | `ClientService.cs` |
| Repository | PascalCase + `Repository` | `ClientRepository.cs` |
| DTO | PascalCase + `Dto` | `CreateClientDto.cs` |
| Entité | PascalCase | `Client.cs` |
| Interface | `I` + PascalCase | `IClientRepository.cs` |
| Migration | Description en PascalCase | `20240901_AddClientTable.cs` |

---

## Structure des dossiers

```
Pierre.Web/
├── Pages/
│   ├── Public/             ← pages sans authentification
│   └── Admin/              ← pages protégées par [Authorize]
│       ├── Dashboard/
│       ├── Clients/        ← Index, Create, Edit, Detail
│       ├── Appointments/   ← Index, Pending, Detail
│       ├── Contents/       ← Index, Create, Edit
│       ├── Planning/       ← Index, CreateSlot
│       ├── AdminTracking/  ← Index, Create, Edit
│       └── Import/         ← Index, Preview, Result
├── Application/
│   ├── Services/           ← un fichier par service
│   ├── DTOs/               ← regroupés par module
│   │   ├── Clients/
│   │   ├── Appointments/
│   │   ├── Contents/
│   │   └── ...
│   └── Interfaces/         ← IClientRepository, IEmailService, etc.
├── Domain/
│   ├── Entities/           ← une entité par fichier
│   └── Enums/              ← un fichier par énumération ou regroupé
├── Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── Migrations/
│   │   └── Repositories/   ← implémentations des interfaces
│   ├── Email/
│   │   ├── SmtpEmailService.cs
│   │   └── Templates/
│   ├── Storage/
│   │   └── LocalFileStorageService.cs
│   └── Import/
│       └── ExcelImportService.cs
└── wwwroot/
    ├── css/
    ├── js/
    └── uploads/
```

---

## Patterns

### Repository
- Une interface par entité principale : `IClientRepository`, `IAppointmentRepository`.
- Méthodes standards : `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`.
- Les repositories ne contiennent pas de logique métier, seulement des requêtes.

### Service applicatif
- Chaque use case correspond à une méthode de service.
- Les services appellent les repositories, appliquent les règles métier et retournent des DTOs.
- Les services ne retournent jamais d'entités Domain directement aux pages.

### DTOs
- Un DTO d'entrée (`CreateXxxDto`, `UpdateXxxDto`) par opération d'écriture.
- Un DTO de sortie (`XxxDto`, `XxxListItemDto`, `XxxDetailDto`) par opération de lecture.
- Les DTOs ne contiennent pas de logique.

### Razor Pages
- Le `PageModel` contient uniquement la logique de liaison et de navigation.
- Il délègue tout le traitement aux services applicatifs.
- Les validations de formulaire utilisent les Data Annotations sur les DTOs.

---

## Base de données

### Nommage des tables
```
snake_case au pluriel (convention PostgreSQL)
clients, appointments, consultation_notes, content_posts
```

### Nommage des colonnes
```
snake_case
first_name, is_archived, published_at
```

### Clés primaires
```
Toujours un Guid, nommé id
```

### Clés étrangères
```
nom_de_l_entité_id
client_id, appointment_id
```

### Soft delete
- Les entités supportant l'archivage utilisent `is_archived` (bool) et non une suppression physique.
- Les requêtes de liste filtrent par défaut sur `is_archived = false`.

---

## Validation

- Validation des formulaires avec `DataAnnotations` sur les DTOs (`[Required]`, `[MaxLength]`, `[EmailAddress]`).
- Validation métier dans les services (ex: vérifier qu'un créneau n'est pas déjà bloqué).
- Les messages d'erreur sont en français.

---

## Gestion des erreurs

- Les services lèvent des exceptions typées définies dans `Domain/Exceptions/`.
- Exemples : `NotFoundException`, `ConflictException`, `ValidationException`.
- Les pages Razor catchent ces exceptions et affichent un message utilisateur adapté.
- Les erreurs inattendues sont journalisées via Serilog avant d'être propagées.

---

## Async / Await

- Toutes les méthodes d'accès aux données sont asynchrones (suffixe `Async`).
- Les méthodes de service qui appellent des méthodes async sont elles-mêmes async.
- Utiliser `ConfigureAwait(false)` dans les couches Infrastructure et Application.

---

## Injection de dépendances

- Enregistrement dans `Program.cs` via les méthodes d'extension.
- Les services sont enregistrés en `Scoped` (cycle de vie par requête HTTP).
- Les services stateless peuvent être enregistrés en `Singleton`.

---

## Configuration

- Les paramètres sont lus via `IOptions<T>` avec des classes de configuration typées.
- Exemple : `EmailSettings`, `StorageSettings`.
- Les valeurs sensibles ne sont jamais commitées dans le dépôt.

---

## Commits Git

Format : `type(scope): message en français`

Types :
- `feat` : nouvelle fonctionnalité
- `fix` : correction de bug
- `refactor` : refactoring sans changement de comportement
- `style` : formatage, pas de logique
- `test` : ajout ou modification de tests
- `chore` : tâches de maintenance (dépendances, config)

Exemples :
```
feat(clients): ajouter la création de fiche client
fix(appointments): corriger le blocage de créneau après acceptation
refactor(content): extraire la logique de génération de slug
```

---

## Tests

- Les tests unitaires couvrent les services applicatifs et les règles métier Domain.
- Les tests d'intégration couvrent les repositories avec une base de test en mémoire.
- Convention de nommage : `NomDeLaMéthode_Contexte_RésultatAttendu`.
- Exemple : `CreateAppointment_WhenSlotIsBlocked_ShouldThrowConflictException`.
