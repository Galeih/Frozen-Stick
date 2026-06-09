# Besoins fonctionnels — Projet Pierre (Diététicien)

## Contexte

Plateforme web pour un diététicien indépendant, composée de deux espaces :
- **Site vitrine public** : visibilité, contenu, prise de contact.
- **Back-office privé** : gestion clients, rendez-vous, consultations, contenus.

Le professionnel travaille seul. La solution doit être simple, autonome et administrable sans équipe technique.

---

## Acteurs

| Acteur | Description |
|---|---|
| **Professionnel** | Seul utilisateur interne. Accès complet au back-office. |
| **Visiteur** | Consulte le site public sans compte. Peut demander un rendez-vous. |
| **Client** | Personne suivie. Pas d'espace personnel dans le MVP. |

---

## MVP — Périmètre inclus

### Site vitrine public
- Page d'accueil avec présentation de l'activité.
- Page de présentation du professionnel.
- Page des prestations.
- Page de contact (téléphone, email, formulaire).
- Liens vers les réseaux sociaux.
- Affichage des contenus publiés (recettes, articles, ateliers, conseils, actualités).
- Page de détail d'un contenu.
- Formulaire de demande de rendez-vous.
- Affichage des créneaux disponibles.

### Back-office (authentifié)
- Connexion sécurisée / déconnexion.
- Tableau de bord avec synthèse de l'activité.
- Gestion des contenus (création, modification, publication, archivage).
- Gestion des demandes de rendez-vous (accepter / refuser).
- Gestion du planning et des disponibilités.
- Gestion des clients (création, modification, archivage, recherche).
- Notes de consultation et recommandations par client.
- Suivi administratif simple (factures et références).
- Import de données depuis un fichier Excel.
- Notifications email automatiques.

---

## MVP — Périmètre exclu

- Espace client avec compte personnel.
- Paiement en ligne.
- Synchronisation Google Calendar.
- SMS / WhatsApp.
- Statistiques avancées.
- Newsletter automatisée.
- Application mobile native.
- Comptabilité complète.

---

## Règles métier

### Contenus
- Un contenu a toujours un titre (3 à 120 caractères).
- Un contenu a un type parmi : `Recipe`, `Article`, `News`, `Workshop`, `Tip`.
- Un contenu a un statut `Draft` ou `Published`.
- Seuls les contenus `Published` sont visibles sur le site public.
- Le professionnel peut modifier ou supprimer ses contenus.

### Demande de rendez-vous
- Le visiteur n'a pas besoin de compte pour faire une demande.
- Champs obligatoires : nom, prénom, et au moins un moyen de contact (email OU téléphone).
- Une demande créée a le statut `Pending`.
- Le professionnel doit valider ou refuser chaque demande manuellement.
- Un créneau associé à un rendez-vous `Accepted` devient `IsBlocked = true`.
- Un rendez-vous `Refused` ne bloque pas le créneau.
- Lors de l'acceptation : créer ou associer une fiche client, envoyer un email de confirmation.
- Lors du refus : le client peut être notifié par email.

### Clients
- Un client est identifié par nom, prénom, et au moins un moyen de contact.
- Un client peut être archivé (soft delete, `IsArchived = true`).
- Les clients archivés n'apparaissent pas dans les listes par défaut.
- Un client archivé reste consultable via un filtre dédié.

### Notes de consultation
- Une note est liée à un client obligatoirement.
- Une note peut être liée à un rendez-vous (optionnel).
- Une note contient une date, un contenu libre, et optionnellement des recommandations et un poids.
- Les notes sont privées : jamais visibles sur le site public.
- Seul le professionnel connecté peut lire, créer ou modifier des notes.

### Planning / Disponibilités
- Le professionnel crée des créneaux de disponibilité avec date, heure de début et heure de fin.
- Un créneau peut être bloqué manuellement (`IsBlocked = true`).
- Un créneau bloqué n'apparaît pas dans la liste publique des créneaux disponibles.
- Deux rendez-vous acceptés ne peuvent pas se chevaucher sur le même créneau.

### Import Excel
- L'import est déclenché manuellement par le professionnel.
- Une prévisualisation est affichée avant tout enregistrement.
- Les lignes invalides sont signalées avec un message explicite.
- Les doublons sont détectés (même nom + même email ou téléphone).
- Les données ne sont enregistrées qu'après confirmation explicite du professionnel.
- Un rapport d'import est généré (lignes importées, erreurs, doublons).

### Sécurité
- Toutes les routes `/Admin/` nécessitent une authentification.
- Un visiteur non connecté est redirigé vers la page de connexion.
- Les mots de passe ne sont jamais stockés en clair.
- Les données privées (notes, clients, planning) ne sont jamais exposées via les routes publiques.

---

## User Stories

### US-001 — Consulter le site vitrine
**En tant que** visiteur, **je veux** consulter le site vitrine **afin de** découvrir l'activité du professionnel.

Critères d'acceptation :
- Le site est accessible sans connexion.
- Les informations principales sont visibles depuis la page d'accueil.
- Les liens réseaux sociaux fonctionnent.
- Les contenus non publiés ne sont pas affichés.

---

### US-002 — Publier un contenu
**En tant que** professionnel connecté, **je veux** publier un contenu **afin de** partager des recettes, articles ou ateliers.

Critères d'acceptation :
- Un contenu publié apparaît sur le site public.
- Un contenu en brouillon reste privé.
- Le professionnel peut modifier ou dépublier un contenu existant.
- Le professionnel peut filtrer les contenus par type ou statut.

---

### US-003 — Demander un rendez-vous
**En tant que** visiteur, **je veux** demander un rendez-vous **afin de** prendre contact sans passer par téléphone.

Critères d'acceptation :
- Une demande valide est enregistrée avec le statut `Pending`.
- Une demande sans email ni téléphone affiche une erreur.
- Le professionnel est notifié par email.
- Le visiteur reçoit un message de confirmation de dépôt.

---

### US-004 — Accepter ou refuser un rendez-vous
**En tant que** professionnel connecté, **je veux** traiter les demandes de rendez-vous **afin de** confirmer ou refuser un créneau.

Critères d'acceptation :
- Le professionnel voit les demandes en attente.
- L'acceptation bloque le créneau et envoie un email de confirmation.
- Le refus laisse le créneau disponible.
- Une fiche client est créée ou associée lors de l'acceptation.

---

### US-005 — Ajouter une note de consultation
**En tant que** professionnel connecté, **je veux** ajouter une note de consultation **afin de** conserver l'historique du suivi.

Critères d'acceptation :
- La note est liée au client concerné.
- L'historique est consultable dans la fiche client.
- Les notes sont invisibles sur le site public.

---

### US-006 — Gérer les clients
**En tant que** professionnel connecté, **je veux** gérer mes fiches clients **afin de** centraliser les informations.

Critères d'acceptation :
- Un client peut être créé, modifié, archivé.
- La recherche par nom fonctionne.
- Les clients archivés sont masqués par défaut.
- Les rendez-vous et notes associés sont visibles dans la fiche.

---

### US-007 — Consulter le tableau de bord
**En tant que** professionnel connecté, **je veux** voir un tableau de bord **afin d'avoir** une vue rapide de mon activité.

Critères d'acceptation :
- Affiche les prochains rendez-vous.
- Affiche les demandes en attente.
- Affiche les derniers clients ajoutés.
- Affiche les derniers contenus publiés.
- Les éléments sont cliquables vers les pages détaillées.

---

### US-008 — Importer des données Excel
**En tant que** professionnel connecté, **je veux** importer mon ancien fichier Excel **afin de** récupérer l'historique client.

Critères d'acceptation :
- Un fichier `.xlsx` valide peut être analysé.
- Une prévisualisation est affichée avant import.
- Les lignes invalides et doublons sont signalés.
- Les données ne sont enregistrées qu'après confirmation.
- Un rapport d'import est généré.

---

## Technical Stories indispensables

| ID | Sujet |
|---|---|
| TS-001 | Mettre en place l'architecture du projet (structure en couches) |
| TS-002 | Mettre en place l'authentification (ASP.NET Core Identity) |
| TS-003 | Créer le modèle de données et les migrations EF Core |
| TS-004 | Configurer la base de données et les seeds de développement |
| TS-005 | Créer le module de gestion des contenus |
| TS-006 | Créer le module de rendez-vous et planning |
| TS-007 | Mettre en place les notifications email |
| TS-008 | Créer le module de gestion des clients |
| TS-009 | Créer le module de suivi de consultation |
| TS-010 | Créer le tableau de bord |
| TS-011 | Créer le module d'import Excel |
| TS-012 | Mettre en place la journalisation des actions sensibles |

---

## Priorisation MVP

| Priorité | Fonctionnalité |
|---|---|
| P1 | Connexion administrateur |
| P1 | Back-office sécurisé |
| P1 | Site vitrine public |
| P1 | Gestion des contenus (CRUD + publication) |
| P1 | Demande de rendez-vous publique |
| P1 | Validation / refus des rendez-vous |
| P1 | Gestion des clients |
| P1 | Notes de consultation |
| P2 | Notifications email |
| P2 | Tableau de bord |
| P2 | Gestion avancée du planning |
| P2 | Suivi administratif simple |
| P3 | Import Excel avancé |
| P3 | Espace client |
| P3 | SMS / WhatsApp |
| P3 | Newsletter automatisée |
| P3 | Paiement en ligne |
