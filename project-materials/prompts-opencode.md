# Prompts OpenCode — Projet Pierre (Diététicien)

Chaque prompt est à copier-coller tel quel dans OpenCode.
Attendre que chaque étape soit terminée et validée avant de passer à la suivante.

---

## PROMPT 0 — Initialisation du contexte

```
Avant de commencer quoi que ce soit, lis attentivement ces quatre fichiers qui définissent 
l'intégralité du projet :

- architecture.md  → stack technique, structure des dossiers, entités, relations, config
- besoin.md        → besoins fonctionnels, règles métier, user stories, MVP
- convention.md    → nommage, patterns, structure, conventions Git
- design.md        → pages, UX, composants, responsive

Confirme que tu les as lus en résumant en 5 points clés ce que tu as retenu de chaque fichier.
Ne génère aucun code pour l'instant.
```

---

## PROMPT 1 — TS-001 : Initialisation du projet

```
Implémente TS-001 de besoin.md : mise en place de l'architecture du projet.

À partir de architecture.md et convention.md, crée :

1. Le projet ASP.NET Core 8 avec Razor Pages nommé "Pierre.Web".
2. La structure exacte des dossiers définie dans architecture.md :
   Pages/Public/, Pages/Admin/, Application/Services/, Application/DTOs/, 
   Application/Interfaces/, Domain/Entities/, Domain/Enums/, Domain/Exceptions/,
   Infrastructure/Data/, Infrastructure/Email/, Infrastructure/Storage/, Infrastructure/Import/
3. Les packages NuGet nécessaires :
   - Microsoft.EntityFrameworkCore
   - Microsoft.EntityFrameworkCore.Design
   - Npgsql.EntityFrameworkCore.PostgreSQL
   - Microsoft.AspNetCore.Identity.EntityFrameworkCore
   - Serilog.AspNetCore
   - EPPlus (pour l'import Excel)
   - MailKit (pour les emails)
4. Un fichier appsettings.json avec les sections : ConnectionStrings, EmailSettings, AdminSeed.
5. Un fichier appsettings.Development.json avec une base PostgreSQL locale.
6. La configuration de Serilog dans Program.cs (console + fichier logs/).
7. Les classes de configuration typées : EmailSettings, AdminSeedSettings.
8. Un README.md minimal avec les instructions pour lancer le projet.

Respecte strictement les conventions de nommage de convention.md.

Critères de validation :
- Le projet compile sans erreur.
- La structure des dossiers correspond exactement à architecture.md.
```

---

## PROMPT 2 — TS-002 : Authentification

```
Implémente TS-002 de besoin.md : authentification avec ASP.NET Core Identity.

À partir de architecture.md (section Authentification) et convention.md :

1. Crée l'entité ApplicationUser héritant d'IdentityUser dans Domain/Entities/.
2. Configure ASP.NET Core Identity dans Program.cs avec cookie d'authentification.
3. Crée le AppDbContext dans Infrastructure/Data/ héritant d'IdentityDbContext<ApplicationUser>.
4. Crée un DatabaseSeeder dans Infrastructure/Data/ qui :
   - Applique les migrations au démarrage (MigrateAsync).
   - Crée le compte admin depuis AdminSeedSettings si aucun utilisateur n'existe.
5. Crée la Razor Page Pages/Admin/Login.cshtml avec :
   - Formulaire email + mot de passe.
   - Gestion des erreurs d'authentification en français.
   - Redirection vers /Admin/Dashboard après connexion.
6. Crée la Razor Page Pages/Admin/Logout.cshtml (POST uniquement).
7. Protège toutes les pages sous Pages/Admin/ avec [Authorize] via une convention globale dans Program.cs.
8. Redirige les non-authentifiés vers /Admin/Login.
9. Crée le layout Pages/Admin/_AdminLayout.cshtml avec sidebar, topbar et lien de déconnexion.

Critères de validation :
- Un visiteur non connecté accédant à /Admin/Dashboard est redirigé vers /Admin/Login.
- Après connexion avec les bons identifiants, l'accès au Dashboard est accordé.
- Le mot de passe est haché (jamais stocké en clair).
- La déconnexion détruit la session.
```

---

## PROMPT 3 — TS-003 + TS-004 : Modèle de données

```
Implémente TS-003 et TS-004 de besoin.md : entités, relations et base de données.

À partir de architecture.md (sections Entités, Relations, Énumérations) et convention.md 
(section Base de données) :

1. Crée toutes les entités dans Domain/Entities/ avec exactement les propriétés définies 
   dans architecture.md :
   Client, Appointment, Availability, ConsultationNote, ContentPost, 
   ContactRequest, Invoice, ImportedFile, NotificationLog.

2. Crée toutes les énumérations dans Domain/Enums/ :
   AppointmentStatus, ContentType, ContentStatus, InvoiceStatus, ImportStatus.

3. Crée les exceptions métier dans Domain/Exceptions/ :
   NotFoundException, ConflictException, ValidationException.

4. Configure le AppDbContext avec :
   - Toutes les entités en DbSet.
   - Les relations définies dans architecture.md (Client→Appointment, Client→ConsultationNote, etc.).
   - Les noms de tables en snake_case selon convention.md.
   - Les noms de colonnes en snake_case.
   - Les contraintes (required, maxlength) via Fluent API.
   - Les index sur : Client.LastName, ContentPost.Slug, ContentPost.Status, Appointment.Status.

5. Génère la migration initiale nommée "InitialCreate".

6. Crée un DataSeeder de développement qui insère :
   - 5 clients fictifs.
   - 3 contenus publiés (1 recette, 1 article, 1 atelier).
   - 5 créneaux de disponibilité dans les 2 prochaines semaines.

Critères de validation :
- La migration s'applique sans erreur.
- Toutes les tables existent avec les bonnes colonnes.
- Les relations sont correctement configurées (clés étrangères).
- Les données de seed s'insèrent sans erreur.
```

---

## PROMPT 4 — TS-005 + US-002 : Module Contenus

```
Implémente TS-005 et US-002 : module de gestion des contenus.

À partir de besoin.md (US-002 et règles métier Contenus), convention.md (patterns Repository/Service/DTO) 
et design.md (sections "Gestion des contenus" et "Page Blog / Contenus") :

1. Crée l'interface IContentRepository dans Application/Interfaces/ avec :
   GetByIdAsync, GetAllAsync, GetPublishedAsync, GetBySlugAsync, AddAsync, UpdateAsync, DeleteAsync.

2. Crée l'implémentation ContentRepository dans Infrastructure/Data/Repositories/.

3. Crée les DTOs dans Application/DTOs/Contents/ :
   ContentListItemDto, ContentDetailDto, CreateContentDto, UpdateContentDto.

4. Crée ContentService dans Application/Services/ avec :
   GetAllForAdminAsync, GetPublishedAsync, GetBySlugAsync, CreateAsync, UpdateAsync, 
   PublishAsync, UnpublishAsync, DeleteAsync.
   La méthode CreateAsync génère automatiquement le Slug depuis le titre.

5. Crée les Razor Pages Admin :
   - Pages/Admin/Contents/Index.cshtml : liste avec filtres type/statut, boutons Modifier/Publier/Supprimer.
   - Pages/Admin/Contents/Create.cshtml : formulaire de création.
   - Pages/Admin/Contents/Edit.cshtml : formulaire de modification.

6. Crée les Razor Pages publiques :
   - Pages/Public/Contents/Index.cshtml : grille de contenus publiés avec filtres par type.
   - Pages/Public/Contents/Detail.cshtml : page de détail par slug.

7. Enregistre IContentRepository et ContentService dans Program.cs.

8. Crée les tests unitaires de ContentService dans un projet Pierre.Tests/ couvrant :
   - CreateAsync avec titre valide.
   - CreateAsync avec titre trop court (doit lever ValidationException).
   - PublishAsync d'un contenu existant.
   - GetBySlugAsync avec un slug inexistant (doit lever NotFoundException).

Critères de validation selon US-002 :
- Un contenu publié apparaît sur /contenus.
- Un contenu en brouillon n'apparaît pas sur le site public.
- Le professionnel peut créer, modifier, publier, dépublier un contenu depuis le back-office.
- Les filtres par type et statut fonctionnent dans le back-office.
```

---

## PROMPT 5 — US-001 : Site vitrine public

```
Implémente US-001 : site vitrine public.

À partir de besoin.md (US-001), design.md (sections "Site vitrine public", "Navigation", 
"Page d'accueil", "Page Présentation", "Page Prestations", "Page Contact") :

1. Crée le layout public Pages/Public/_PublicLayout.cshtml avec :
   - Navigation sticky : Logo, Accueil, Présentation, Prestations, Blog, Contact, bouton "Prendre RDV".
   - Menu hamburger sur mobile.
   - Footer avec liens, réseaux sociaux, mentions légales.

2. Crée Pages/Public/Index.cshtml (page d'accueil) avec les sections :
   - Hero : accroche + bouton "Prendre rendez-vous" + bouton "En savoir plus".
   - Présentation rapide (2-3 phrases statiques).
   - 3 cartes de prestations (statiques).
   - 3 derniers contenus publiés (dynamiques, via ContentService).
   - Bandeau call-to-action "Prenez rendez-vous".

3. Crée Pages/Public/Presentation.cshtml (contenu statique pour le MVP).

4. Crée Pages/Public/Services.cshtml (liste de prestations statiques).

5. Crée Pages/Public/Contact.cshtml avec :
   - Téléphone cliquable (tel:).
   - Email cliquable (mailto:).
   - Icônes réseaux sociaux.
   - Formulaire de contact simple (nom, email, message) qui enregistre un ContactRequest.

6. Crée un CSS custom dans wwwroot/css/site.css avec :
   - Palette verts / beiges / blanc cassé.
   - Variables CSS pour les couleurs principales.
   - Styles de base responsive (mobile-first).

Critères de validation selon US-001 :
- Le site est accessible sans connexion.
- Les contenus non publiés n'apparaissent pas.
- Les liens de navigation fonctionnent.
- La page s'affiche correctement sur mobile (vérifier avec DevTools).
```

---

## PROMPT 6 — TS-006 + US-003 : Demande de rendez-vous publique

```
Implémente TS-006 (partie publique) et US-003 : demande de rendez-vous depuis le site.

À partir de besoin.md (US-003 et règles métier Demande de rendez-vous, Planning), 
architecture.md (entités Appointment, Availability) et design.md (section "Page Demande de rendez-vous") :

1. Crée IAvailabilityRepository et IAppointmentRepository dans Application/Interfaces/.

2. Crée les implémentations dans Infrastructure/Data/Repositories/.

3. Crée les DTOs dans Application/DTOs/Appointments/ :
   AvailabilitySlotDto, AppointmentRequestDto, AppointmentListItemDto, AppointmentDetailDto.

4. Crée AppointmentService dans Application/Services/ avec :
   GetAvailableSlotsAsync : retourne les créneaux non bloqués et non réservés dans les 4 prochaines semaines.
   RequestAppointmentAsync : valide le DTO (nom + prénom + email OU téléphone obligatoires), 
   enregistre la demande avec statut Pending, déclenche la notification email au professionnel.

5. Crée l'interface IEmailService dans Application/Interfaces/ avec SendAsync(to, subject, body).

6. Crée SmtpEmailService dans Infrastructure/Email/ avec MailKit.

7. Crée les templates email dans Infrastructure/Email/Templates/ :
   - NewAppointmentRequest.txt : notification au professionnel.
   - AppointmentConfirmed.txt : confirmation au client.
   - AppointmentRefused.txt : refus au client.

8. Crée Pages/Public/Booking/Request.cshtml avec :
   - Étape 1 : liste des créneaux disponibles (date + heure, sélection par radio button).
   - Étape 2 : formulaire nom, prénom, email, téléphone, message.
   - Récapitulatif du créneau choisi.
   - Message de confirmation après envoi.
   - Gestion du cas "aucun créneau disponible".

9. Crée les tests unitaires de AppointmentService :
   - RequestAppointmentAsync avec données valides.
   - RequestAppointmentAsync sans email ni téléphone (doit lever ValidationException).
   - GetAvailableSlotsAsync ne retourne pas les créneaux bloqués.

Critères de validation selon US-003 :
- Une demande valide est enregistrée avec statut Pending.
- Une demande sans contact affiche une erreur en français.
- Le professionnel reçoit un email de notification.
- Le visiteur voit un message de confirmation.
- Les créneaux bloqués ou passés ne s'affichent pas.
```

---

## PROMPT 7 — US-004 : Validation des rendez-vous (back-office)

```
Implémente US-004 : accepter ou refuser une demande de rendez-vous depuis le back-office.

À partir de besoin.md (US-004 et règles métier), design.md (section "Gestion des rendez-vous") :

1. Ajoute dans AppointmentService :
   GetPendingAsync : liste des demandes en attente.
   GetAllForAdminAsync : toutes les demandes avec filtre par statut.
   AcceptAsync(id) : 
     - Passe le statut à Accepted.
     - Bloque le créneau (IsBlocked = true sur l'Availability).
     - Crée une fiche Client si aucun client avec le même email ou téléphone n'existe déjà.
     - Associe le rendez-vous au client.
     - Envoie l'email de confirmation au demandeur.
   RefuseAsync(id) :
     - Passe le statut à Refused.
     - Ne bloque pas le créneau.
     - Envoie optionnellement l'email de refus.

2. Crée les Razor Pages Admin :
   - Pages/Admin/Appointments/Index.cshtml : liste avec onglets Toutes / En attente / Acceptées / Refusées.
     Boutons "Accepter" et "Refuser" directement dans la ligne pour les demandes Pending.
   - Pages/Admin/Appointments/Detail.cshtml : détail complet avec informations du demandeur,
     client associé, et boutons d'action.

3. Ajoute la gestion du planning :
   - Pages/Admin/Planning/Index.cshtml : vue liste des créneaux (date, heure, statut).
   - Pages/Admin/Planning/CreateSlot.cshtml : formulaire date + heure début + heure fin + option bloquer.
   - Valide qu'un nouveau créneau ne chevauche pas un créneau existant le même jour.

4. Ajoute les tests unitaires :
   - AcceptAsync bloque bien le créneau.
   - AcceptAsync crée un client si inexistant.
   - AcceptAsync ne crée pas de doublon client si l'email existe déjà.
   - RefuseAsync ne bloque pas le créneau.

Critères de validation selon US-004 :
- Le professionnel voit les demandes en attente sur le tableau de bord et dans la liste.
- L'acceptation bloque le créneau et envoie l'email de confirmation.
- Le refus laisse le créneau disponible.
- Une fiche client est créée ou associée lors de l'acceptation.
```

---
## PROMPT 8 — TS-008 + US-006 : Gestion des clients

```
Implémente TS-008 et US-006 : module de gestion des clients.

À partir de besoin.md (US-006 et règles métier Clients), convention.md et design.md 
(section "Gestion des clients") :

1. Crée IClientRepository dans Application/Interfaces/ avec :
   GetByIdAsync, GetAllActiveAsync, GetAllIncludingArchivedAsync, SearchAsync(query), 
   FindByContactAsync(email, phone), AddAsync, UpdateAsync, ArchiveAsync.

2. Crée l'implémentation ClientRepository avec :
   - GetAllActiveAsync filtre IsArchived = false.
   - SearchAsync cherche sur FirstName + LastName (case-insensitive).
   - FindByContactAsync cherche par email OU téléphone.

3. Crée les DTOs dans Application/DTOs/Clients/ :
   ClientListItemDto, ClientDetailDto, CreateClientDto, UpdateClientDto.

4. Crée ClientService avec :
   GetAllAsync(includeArchived), SearchAsync, GetByIdAsync, CreateAsync, UpdateAsync, ArchiveAsync.

5. Crée les Razor Pages Admin :
   - Pages/Admin/Clients/Index.cshtml : tableau avec colonnes nom/prénom/email/téléphone/date création.
     Recherche par nom. Filtre "Afficher les archivés". Bouton "Nouveau client". Bouton "Voir" par ligne.
   - Pages/Admin/Clients/Detail.cshtml : fiche complète avec informations, historique des rendez-vous,
     historique des notes de consultation, boutons Modifier et Archiver.
     Confirmation avant archivage (modal ou page de confirmation).
   - Pages/Admin/Clients/Create.cshtml : formulaire prénom, nom, email, téléphone, date de naissance, notes.
   - Pages/Admin/Clients/Edit.cshtml : même formulaire en modification.

6. Crée les tests unitaires de ClientService :
   - CreateAsync avec données valides.
   - ArchiveAsync passe IsArchived à true.
   - GetAllAsync(includeArchived: false) ne retourne pas les clients archivés.
   - SearchAsync retourne les clients correspondant à la recherche.

Critères de validation selon US-006 :
- Un client peut être créé, modifié, archivé.
- La recherche par nom fonctionne.
- Les clients archivés sont masqués par défaut et visibles avec le filtre.
- La fiche client affiche l'historique des rendez-vous associés.
```

---




==**Arrêt ici le 09/06/2026**==




## PROMPT 9 — TS-009 + US-005 : Notes de consultation

```
Implémente TS-009 et US-005 : module de suivi de consultation.

À partir de besoin.md (US-005 et règles métier Notes de consultation), convention.md 
et design.md (section fiche client, partie notes) :

1. Crée IConsultationNoteRepository dans Application/Interfaces/ avec :
   GetByClientIdAsync(clientId), GetByIdAsync, AddAsync, UpdateAsync.

2. Crée l'implémentation ConsultationNoteRepository.

3. Crée les DTOs dans Application/DTOs/ConsultationNotes/ :
   ConsultationNoteListItemDto, ConsultationNoteDetailDto, CreateConsultationNoteDto, UpdateConsultationNoteDto.

4. Crée ConsultationNoteService avec :
   GetByClientIdAsync : retourne l'historique chronologique décroissant.
   GetByIdAsync.
   CreateAsync : lie la note au client, valide que le client existe.
   UpdateAsync.

5. Crée les Razor Pages Admin :
   - Pages/Admin/ConsultationNotes/Create.cshtml : formulaire avec date, contenu, recommandations, 
     poids (optionnel), sélection du rendez-vous associé (optionnel parmi les rendez-vous du client).
   - Pages/Admin/ConsultationNotes/Edit.cshtml : modification d'une note existante.
   - Pages/Admin/ConsultationNotes/Detail.cshtml : lecture seule d'une note.

6. Intègre l'historique des notes dans Pages/Admin/Clients/Detail.cshtml :
   - Liste chronologique des notes avec date, aperçu du contenu, lien "Voir".
   - Bouton "Ajouter une note" pré-rempli avec le clientId.

7. Vérifie qu'aucune route publique ne permet d'accéder aux notes de consultation.

8. Crée les tests unitaires de ConsultationNoteService :
   - CreateAsync avec client existant.
   - CreateAsync avec clientId inexistant (doit lever NotFoundException).
   - GetByClientIdAsync retourne les notes dans l'ordre chronologique décroissant.

Critères de validation selon US-005 :
- Une note est liée au client concerné.
- L'historique est consultable dans la fiche client.
- Les notes sont uniquement accessibles depuis le back-office authentifié.
- Aucune route publique n'expose les notes.
```

---

## PROMPT 10 — TS-010 + US-007 : Tableau de bord

```
Implémente TS-010 et US-007 : tableau de bord du back-office.

À partir de besoin.md (US-007), design.md (section "Tableau de bord") :

1. Crée DashboardService dans Application/Services/ qui agrège en un seul appel :
   - Les 5 prochains rendez-vous acceptés (date > aujourd'hui, triés par date ASC).
   - Le nombre de demandes en attente + liste des 5 plus récentes.
   - Les 3 derniers clients créés.
   - Les 3 derniers contenus publiés.
   Retourne un DashboardDto unique.

2. Crée Pages/Admin/Dashboard.cshtml avec les widgets :
   - Widget "Demandes en attente" : compteur mis en avant + liste des 5 demandes avec bouton "Voir".
   - Widget "Prochains rendez-vous" : liste avec client, date, heure.
   - Widget "Derniers clients" : liste avec nom, date d'ajout, lien vers la fiche.
   - Widget "Derniers contenus" : liste avec titre, type, date de publication.
   - Chaque widget a un lien "Voir tout" vers la liste complète.

3. Met à jour _AdminLayout.cshtml pour que le lien "Tableau de bord" de la sidebar 
   pointe vers /Admin/Dashboard.

4. Ajoute dans la sidebar les liens vers tous les modules :
   Dashboard, Clients, Rendez-vous, Planning, Contenus, Suivi administratif, Import.

Critères de validation selon US-007 :
- Toutes les données affichées sont à jour après chaque action.
- Chaque widget est cliquable vers la liste complète correspondante.
- Le tableau de bord s'affiche correctement sur ordinateur et tablette.
- La page se charge en moins de 2 secondes (une seule requête agrégée).
```

---

## PROMPT 11 — EPIC 7 : Suivi administratif

```
Implémente EPIC 7 : suivi administratif simple.

À partir de besoin.md (EPIC 7), architecture.md (entité Invoice) et design.md 
(section "Suivi administratif") :

1. Crée IInvoiceRepository dans Application/Interfaces/.
2. Crée InvoiceRepository dans Infrastructure/Data/Repositories/.
3. Crée les DTOs dans Application/DTOs/Invoices/ :
   InvoiceListItemDto, InvoiceDetailDto, CreateInvoiceDto, UpdateInvoiceDto.
4. Crée InvoiceService avec :
   GetAllAsync(statusFilter), GetByClientIdAsync, GetByIdAsync, CreateAsync, UpdateAsync, UpdateStatusAsync.

5. Crée les Razor Pages Admin :
   - Pages/Admin/AdminTracking/Index.cshtml : tableau avec colonnes référence, client, montant, 
     statut (badge coloré), date. Filtre par statut. Bouton "Nouvelle entrée".
   - Pages/Admin/AdminTracking/Create.cshtml : formulaire client (recherche), référence, montant, 
     statut, date, notes.
   - Pages/Admin/AdminTracking/Edit.cshtml : modification.

6. Intègre un résumé des factures dans la fiche client (Pages/Admin/Clients/Detail.cshtml) :
   liste des factures associées avec montant et statut.

Critères de validation :
- Une entrée administrative peut être créée, modifiée, associée à un client.
- Le filtre par statut fonctionne.
- Le résumé apparaît dans la fiche client.
```

---

## PROMPT 12 — TS-011 + US-008 : Import Excel

```
Implémente TS-011 et US-008 : import de données depuis un fichier Excel.

À partir de besoin.md (US-008 et règles métier Import Excel), architecture.md 
(entité ImportedFile, librairie EPPlus) et design.md (section "Import Excel") :

1. Crée ExcelImportService dans Infrastructure/Import/ avec :
   AnalyzeAsync(stream) : lit le fichier .xlsx, mappe les colonnes attendues 
   (Prénom, Nom, Email, Téléphone, Date de naissance), retourne un ExcelAnalysisResult contenant :
   - Liste de ImportRowDto (données valides).
   - Liste de ImportErrorDto (ligne, colonne, message d'erreur en français).
   - Liste de ImportDuplicateDto (lignes détectées comme doublons par email ou téléphone).
   ImportAsync(rows) : enregistre les lignes valides comme clients après confirmation.
   Génère un ImportedFile avec le rapport final.

2. Crée les DTOs dans Application/DTOs/Import/ :
   ExcelAnalysisResult, ImportRowDto, ImportErrorDto, ImportDuplicateDto, ImportReportDto.

3. Crée les Razor Pages Admin :
   - Pages/Admin/Import/Index.cshtml : zone de dépôt de fichier .xlsx + bouton "Analyser".
   - Pages/Admin/Import/Preview.cshtml : tableau de prévisualisation avec :
     * Lignes valides en vert/blanc.
     * Lignes invalides en rouge avec message d'erreur.
     * Lignes doublons en orange avec indication.
     * Compteurs : total / valides / invalides / doublons.
     * Boutons "Confirmer l'import" (POST) et "Annuler".
   - Pages/Admin/Import/Result.cshtml : rapport final avec compteurs et lien vers la liste des clients.

4. Colonnes attendues dans le fichier Excel (ordre flexible, détection par en-tête) :
   Prénom (obligatoire), Nom (obligatoire), Email, Téléphone, Date de naissance.

5. Crée les tests unitaires d'ExcelImportService :
   - AnalyzeAsync détecte une ligne sans Nom (erreur).
   - AnalyzeAsync détecte un doublon par email.
   - ImportAsync n'enregistre que les lignes valides.

Critères de validation selon US-008 :
- Un fichier valide est analysé et prévisualisé.
- Les lignes invalides sont signalées avec un message explicite.
- Les doublons sont détectés.
- Aucune donnée n'est enregistrée sans confirmation explicite.
- Un rapport est généré après import.
```

---

## PROMPT 13 — TS-012 : Sécurité et journalisation

```
Implémente TS-012 : sécurité complète et journalisation des actions sensibles.

À partir de besoin.md (TS-012, règles métier Sécurité) et architecture.md (section Journalisation) :

1. Crée un AuditService dans Application/Services/ avec :
   LogAsync(action, details) : enregistre dans les logs Serilog avec niveau Information.
   Format : "[AUDIT] {Action} | {Details} | {Timestamp}".

2. Instrumente les services existants pour journaliser :
   - Connexion réussie et échec de connexion (dans le PageModel Login).
   - Création / modification / archivage d'un client.
   - Ajout / modification d'une note de consultation.
   - Acceptation / refus d'un rendez-vous.
   - Publication / dépublication d'un contenu.
   - Démarrage et fin d'un import Excel.
   Les logs ne doivent jamais contenir de mot de passe ni le contenu des notes.

3. Vérifie et renforce la sécurité :
   - Toutes les pages Admin ont bien [Authorize] (vérifier via la convention globale ou attribut).
   - Les actions POST sensibles utilisent AntiForgeryToken.
   - Les inputs sont validés avec DataAnnotations côté serveur (ne pas se fier au client).
   - Aucune donnée privée (notes, clients, planning) n'est accessible via une route publique.
   - Ajouter des headers de sécurité dans Program.cs : X-Content-Type-Options, X-Frame-Options.

4. Ajoute la gestion des erreurs globale dans Program.cs :
   - Page /Error pour les erreurs inattendues.
   - Page /Admin/Error pour les erreurs dans le back-office.
   - Log automatique des exceptions non gérées via Serilog.

Critères de validation :
- Les logs contiennent les actions sensibles avec timestamp.
- Les logs ne contiennent aucun mot de passe ni donnée médicale.
- Un visiteur non authentifié ne peut accéder à aucune page Admin.
- Les formulaires sont protégés contre le CSRF.
```

---

## PROMPT 14 — Validation finale MVP

```
Effectue une validation complète du MVP avant livraison.

Vérifie point par point chaque critère d'acceptation des user stories de besoin.md :

US-001 : Le site public est accessible sans connexion. Les contenus non publiés sont invisibles.
US-002 : Publier / dépublier un contenu fonctionne. Les filtres du back-office fonctionnent.
US-003 : La demande de rendez-vous s'enregistre. L'email de notification part. Les erreurs s'affichent.
US-004 : Accepter bloque le créneau. Refuser ne bloque pas. Le client est créé si inexistant.
US-005 : Une note est liée à un client. L'historique est consultable. Les notes sont privées.
US-006 : CRUD client fonctionne. La recherche fonctionne. L'archivage fonctionne.
US-007 : Le tableau de bord affiche toutes les sections. Les données sont à jour.
US-008 : L'import prévisualise, détecte les erreurs et doublons, confirme avant enregistrement.

Pour chaque point qui échoue, liste précisément ce qui manque ou ce qui est cassé.
Génère ensuite un rapport de validation au format :

✅ US-001 : conforme
❌ US-003 : email de notification non envoyé (SmtpEmailService non configuré en dev)
...

Ne marque pas une US comme conforme si un seul critère d'acceptation n'est pas satisfait.
```

---

## PROMPT 15 — Corrections et ajustements finaux

```
Sur la base du rapport de validation précédent, corrige tous les points marqués ❌.

Pour chaque correction :
1. Identifie la cause racine (pas juste le symptôme).
2. Applique la correction minimale nécessaire.
3. Vérifie que la correction ne casse pas les autres US.
4. Relance la vérification du critère concerné.

Une fois toutes les corrections appliquées, génère la liste des commandes pour :
- Lancer le projet en développement.
- Appliquer les migrations.
- Créer le compte admin initial.
- Lancer les tests unitaires.
```
