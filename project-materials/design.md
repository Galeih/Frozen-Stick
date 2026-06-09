# Design & UX — Projet Pierre (Diététicien)

## Principes généraux

- Design sobre, lisible, professionnel. Adapté à un professionnel de santé.
- Responsive : fonctionne sur ordinateur, tablette et mobile.
- Pas de framework JavaScript lourd. HTML / CSS / JS vanilla ou Alpine.js léger si nécessaire.
- CSS : TailwindCSS ou un fichier CSS custom. Pas de Bootstrap surchargé.
- Accessibilité : contrastes suffisants, labels sur les formulaires, navigation clavier possible.
- Palette de couleurs : tons verts / beiges / blancs cassés — univers nutrition, naturel, santé.

---

## Site vitrine public

### Navigation
```
Logo / Nom          [Accueil] [Présentation] [Prestations] [Blog] [Ateliers] [Contact] [Prendre RDV →]
```
- Menu fixe en haut, sticky sur scroll.
- Sur mobile : hamburger menu.
- Le bouton "Prendre RDV" est mis en avant (couleur d'accent, légèrement différent des autres liens).

---

### Page d'accueil (`/`)

Sections dans l'ordre :
1. **Hero** : photo du professionnel + accroche courte + bouton "Prendre rendez-vous" + bouton "En savoir plus".
2. **Présentation rapide** : 2-3 phrases sur l'activité, lien vers la page Présentation.
3. **Prestations** : 3 cartes (accompagnement, consultation, atelier) avec icône, titre, description courte.
4. **Derniers contenus** : grille de 3 contenus récents (recettes / articles) avec image, titre, type, lien.
5. **Témoignages** (optionnel, statique dans le MVP).
6. **Call to action** : bandeau "Prenez rendez-vous" + bouton.
7. **Footer** : liens, réseaux sociaux, mentions légales.

---

### Page Présentation (`/presentation`)

- Photo du professionnel.
- Texte de présentation (géré depuis le back-office comme un contenu de type spécial, ou texte statique en MVP).
- Formation / parcours (liste ou texte libre).
- Valeurs / approche.
- Lien vers les prestations.

---

### Page Prestations (`/prestations`)

- Liste des prestations avec titre, description, durée, tarif (optionnel).
- Chaque prestation dans une carte ou section.
- Bouton "Prendre rendez-vous" à la fin.

---

### Page Blog / Contenus (`/contenus`)

- Filtres par type : Tout / Recettes / Articles / Actualités / Ateliers / Conseils.
- Grille de cartes : image de couverture, type (badge), titre, date, lien vers le détail.
- Pagination ou chargement en bas de page.

### Page détail d'un contenu (`/contenus/{slug}`)

- Titre, type, date de publication.
- Image de couverture.
- Corps du contenu (HTML rendu).
- Liens de partage (optionnel).
- Navigation vers le contenu précédent / suivant.

---

### Page Contact (`/contact`)

- Numéro de téléphone (cliquable sur mobile).
- Adresse email (cliquable).
- Liens réseaux sociaux (icônes).
- Formulaire de contact simple : nom, email, message, bouton Envoyer.
- Message de confirmation après envoi.

---

### Page Demande de rendez-vous (`/rendez-vous`)

Étape 1 — Choix du créneau :
- Calendrier ou liste de créneaux disponibles.
- Les créneaux bloqués ou déjà pris ne sont pas affichés.
- L'utilisateur sélectionne un créneau.

Étape 2 — Informations personnelles :
- Nom (obligatoire).
- Prénom (obligatoire).
- Email (obligatoire si pas de téléphone).
- Téléphone (obligatoire si pas d'email).
- Message optionnel.
- Récapitulatif du créneau choisi.
- Bouton "Envoyer ma demande".

Confirmation :
- Message : "Votre demande a bien été envoyée. Vous recevrez une confirmation par email sous 24h."
- Pas de redirection immédiate vers un espace client.

Règles d'affichage :
- Les créneaux s'affichent pour les 4 prochaines semaines.
- Un créneau passé n'est jamais affiché.
- Si aucun créneau disponible : message "Aucun créneau disponible pour le moment. Contactez-moi directement."

---

## Back-office (pages Admin)

### Connexion (`/Admin/Login`)

- Formulaire centré : email + mot de passe + bouton "Se connecter".
- Message d'erreur si identifiants incorrects.
- Pas de lien "Mot de passe oublié" dans le MVP (réinitialisation manuelle).

---

### Tableau de bord (`/Admin/Dashboard`)

Widgets affichés :
- **Demandes en attente** : nombre + liste des 5 plus récentes avec bouton "Voir".
- **Prochains rendez-vous** : liste des 5 prochains avec client, date, heure.
- **Derniers clients ajoutés** : liste des 3 derniers.
- **Derniers contenus publiés** : liste des 3 derniers.
- **Éléments administratifs récents** : factures récentes ou en attente.

Chaque widget est cliquable vers la liste complète correspondante.

---

### Gestion des clients (`/Admin/Clients`)

**Liste** :
- Tableau : nom, prénom, email, téléphone, date de création, statut (actif / archivé).
- Recherche par nom ou prénom.
- Filtre "Afficher les archivés".
- Bouton "Nouveau client".
- Bouton "Voir" sur chaque ligne.

**Fiche client** (`/Admin/Clients/{id}`) :
- Informations personnelles (nom, prénom, email, téléphone, date de naissance, notes libres).
- Historique des rendez-vous (date, statut).
- Historique des notes de consultation (date, aperçu).
- Historique des recommandations.
- Boutons : Modifier, Archiver.

**Création / Modification** :
- Formulaire : prénom, nom, email, téléphone, date de naissance, notes.
- Validation inline.

---

### Gestion des rendez-vous (`/Admin/Appointments`)

**Liste** :
- Onglets : Toutes / En attente / Acceptées / Refusées.
- Colonnes : date, créneau, demandeur, statut, actions.
- Bouton "Accepter" / "Refuser" directement dans la liste pour les demandes en attente.

**Détail** (`/Admin/Appointments/{id}`) :
- Informations du demandeur.
- Créneau demandé.
- Message laissé.
- Client associé (ou création rapide).
- Boutons : Accepter, Refuser, Annuler.

---

### Gestion du planning (`/Admin/Planning`)

**Vue calendrier** :
- Vue mensuelle ou hebdomadaire.
- Affichage des créneaux créés (disponible / bloqué / réservé).
- Clic sur un jour → ajouter un créneau.

**Créer un créneau** :
- Date, heure de début, heure de fin.
- Option "Bloquer ce créneau" (indisponibilité).

---

### Gestion des contenus (`/Admin/Contents`)

**Liste** :
- Tableau : titre, type, statut (brouillon / publié), date de modification.
- Filtres par type et statut.
- Bouton "Nouveau contenu".
- Boutons : Modifier, Publier/Dépublier, Supprimer.

**Création / Modification** :
- Titre (obligatoire).
- Type (select).
- Statut (radio : Brouillon / Publié).
- Image de couverture (upload).
- Corps (éditeur de texte riche simple : TipTap ou SimpleMDE).
- Bouton Enregistrer.

---

### Suivi administratif (`/Admin/AdminTracking`)

**Liste** :
- Tableau : référence, client, montant, statut (en attente / payé / annulé), date.
- Filtre par statut.
- Bouton "Nouvelle entrée".

**Création** :
- Client (recherche / sélection).
- Référence ou description.
- Montant.
- Statut.
- Date.
- Notes.

---

### Import Excel (`/Admin/Import`)

**Étape 1 — Upload** :
- Zone de dépôt de fichier ou sélection `.xlsx`.
- Bouton "Analyser".

**Étape 2 — Prévisualisation** :
- Tableau des données lues depuis le fichier.
- Lignes valides en blanc, lignes invalides en rouge avec message d'erreur.
- Lignes doublon signalées en orange.
- Compteurs : total / valides / invalides / doublons.
- Boutons : "Confirmer l'import" / "Annuler".

**Étape 3 — Résultat** :
- Rapport : X clients importés, Y erreurs, Z doublons ignorés.
- Lien vers la liste des clients.

---

## Composants réutilisables

| Composant | Description |
|---|---|
| `_Layout.cshtml` | Layout du site public (nav + footer) |
| `_AdminLayout.cshtml` | Layout du back-office (sidebar + topbar) |
| `_ContentCard.cshtml` | Carte de contenu pour la page publique |
| `_ClientRow.cshtml` | Ligne de tableau client |
| `_StatusBadge.cshtml` | Badge coloré selon le statut |
| `_ConfirmModal.cshtml` | Modal de confirmation pour les actions destructives |
| `_Pagination.cshtml` | Composant de pagination |
| `_FlashMessage.cshtml` | Message de succès / erreur en haut de page |

---

## Messages et feedback utilisateur

- Succès (vert) : "Client créé avec succès.", "Rendez-vous accepté.", "Contenu publié."
- Erreur (rouge) : "Un email ou un téléphone est obligatoire.", "Ce créneau est déjà réservé."
- Information (bleu) : "Aucun créneau disponible pour le moment."
- Avertissement (orange) : "Ce client sera archivé et n'apparaîtra plus dans les listes."

Les messages flash sont affichés en haut de page après une action et disparaissent automatiquement après 4 secondes.

---

## Responsive

| Breakpoint | Comportement |
|---|---|
| Mobile (< 640px) | Navigation hamburger, tableaux avec scroll horizontal, formulaires pleine largeur |
| Tablette (640–1024px) | Sidebar réduite ou masquée, grilles en 2 colonnes |
| Desktop (> 1024px) | Sidebar visible, grilles en 3 colonnes, tableaux complets |

Le back-office est principalement conçu pour ordinateur. La version mobile du back-office est fonctionnelle mais simplifiée.
