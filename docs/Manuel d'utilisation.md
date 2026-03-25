# Manuel d'utilisation — Pad'Time

> Version 1.0 — Mars 2026
> Ce document décrit le fonctionnement de la plateforme Pad'Time, côté utilisateur et côté administrateur, en mettant en correspondance chaque fonctionnalité avec la spec métier (P1–P4) et son implémentation technique (backend + frontend).

---

## Table des matières

1. [Contexte et périmètre](#1-contexte-et-périmètre)
2. [Glossaire métier](#2-glossaire-métier)
3. [Profils utilisateurs et droits](#3-profils-utilisateurs-et-droits)
4. [Authentification](#4-authentification)
5. [Réserver un match](#5-réserver-un-match)
6. [Rejoindre un match public](#6-rejoindre-un-match-public)
7. [Mes matchs](#7-mes-matchs)
8. [Paiements](#8-paiements)
9. [Cycle de vie automatique des matchs](#9-cycle-de-vie-automatique-des-matchs)
10. [Administration — Sites et terrains](#10-administration--sites-et-terrains)
11. [Administration — Vue opérationnelle](#11-administration--vue-opérationnelle)
12. [Administration — Analytics revenus](#12-administration--analytics-revenus)
13. [Référence API](#13-référence-api)
14. [Codes d'erreur métier](#14-codes-derreur-métier)

---

## 1. Contexte et périmètre

Pad'Time est une plateforme de réservation de terrains de padel. Elle permet :

- aux **membres** de créer ou rejoindre des matchs sur des créneaux disponibles ;
- aux **administrateurs** de gérer les sites, les terrains, les horaires, les fermetures et de surveiller l'activité opérationnelle.

Le système repose sur trois composants principaux :

| Composant | Rôle |
|-----------|------|
| **IdentityServer** (Duende) | Authentification OIDC / OAuth2, émission des JWT |
| **BackendApi** (.NET 10, CQRS/MediatR) | Logique métier, persistance, endpoints REST |
| **AngularApp** (Angular 19) | Interface utilisateur web |

---

## 2. Glossaire métier

| Terme | Définition |
|-------|-----------|
| **Site** | Unité de gestion regroupant terrains, horaires et fermetures. Périmètre d'autorité d'un admin site. |
| **Terrain** | Ressource physique unique, appartient à un seul site, réservable uniquement via un match. |
| **Créneau (Slot)** | Intervalle de 1h30 avec une pause obligatoire de 15 min. Défini par terrain + heure de début. Un slot ne contient qu'un seul match. |
| **Match** | Réservation d'un terrain sur un créneau. Types : `private` ou `public`. Contient 1 organisateur + 0 à 3 participants. |
| **Organisateur** | Membre créateur du match. Responsable du remplissage et du paiement final. Peut accumuler une dette. |
| **Participant** | Membre inscrit à un match. Doit payer pour valider sa place. |
| **Membre** | Utilisateur authentifié identifié par un matricule (`Gxxxx` / `Sxxxxx` / `Lxxxxx`). |
| **Paiement (mock)** | Transaction simulée liée à un participant. Toujours acceptée (environnement de démonstration). |
| **Dette** | Montant dû par un organisateur suite à un match incomplet. Bloque la création de nouveaux matchs. |

---

## 3. Profils utilisateurs et droits

### 3.1 Catégories membres

| Catégorie | Matricule | Fenêtre de réservation | Sites autorisés |
|-----------|-----------|------------------------|-----------------|
| **Global** | `Gxxxx` | J-21 avant le match | Tous les sites |
| **Site** | `Sxxxxx` | J-14 avant le match | Site assigné uniquement |
| **Libre** | `Lxxxxx` | J-5 avant le match | Tous les sites |

> **Spec (P1 §2)** : La fenêtre de réservation est calculée automatiquement par le backend lors de chaque tentative de création. Un dépassement retourne `403 booking.reservation_window_denied`.

### 3.2 Rôles et autorisations

| Rôle | Accès |
|------|-------|
| `user` | Endpoints utilisateur (booking, paiements, ses matchs) |
| `admin_site` | Endpoints admin limités à son site (`siteId` du token JWT) |
| `admin_global` | Tous les endpoints sans restriction de périmètre |

> **Implémentation backend** : Authorization policies dans `Policies.cs`. Les handlers `SiteAccessHandler` et `SiteManagementHandler` appliquent le filtrage ABAC côté serveur — le frontend ne peut pas contourner ce filtrage.

---

## 4. Authentification

### Flux

1. L'utilisateur clique sur **Connexion** → redirection vers IdentityServer (Authorization Code + PKCE).
2. Après authentification, retour sur `/callback` → le token JWT est stocké côté Angular.
3. Toutes les requêtes API incluent `Authorization: Bearer <jwt>`.

### Claims du token

```
sub          → identifiant technique (OIDC subject)
matricule    → identifiant métier (ex: G1234)
role         → user | admin_site | admin_global
member_category → global | site | free
site_id      → présent si role=admin_site ou member_category=site
```

### Frontend

- **Route** : `/auth/login`, `/callback`
- **Service** : `AuthService` (`core/auth/auth.service.ts`)
- **Guard** : `authGuard` — protège toutes les routes authentifiées

### Backend

- **Endpoint** : `GET /api/v1/me`
- **Handler** : `MeController`
- **Réponse** :
  ```json
  {
    "subject": "abc-123",
    "matricule": "G1234",
    "category": "global",
    "role": "user",
    "siteId": null
  }
  ```

---

## 5. Réserver un match

### Parcours utilisateur

```
[Page Réservation]
  → Choisir un site
  → Choisir un terrain (optionnel)
  → Choisir une date
  → Voir les créneaux disponibles
  → Sélectionner un créneau
  → Configurer le match (type, participants)
  → Confirmer
```

### Frontend

- **Route** : `/booking` (page principale après connexion)
- **Composant principal** : `BookPageComponent`
- **Sous-composants** :
  - `SiteCourtSelectorComponent` — sélection site/terrain
  - `SlotPickerComponent` — affichage des créneaux via `GET /availability`
  - `MatchFormComponent` — formulaire type + participants privés

### Disponibilités

Les créneaux sont calculés à partir des **horaires annuels** et des **fermetures** du site, en excluant les matchs déjà existants (anti double-booking).

**Appel API** :
```
GET /api/v1/availability?siteId={id}&date={YYYY-MM-DD}&courtId={id}
```

**Réponse** :
```json
{
  "siteId": "...",
  "date": "2026-03-25",
  "slots": [
    { "startAt": "2026-03-25T09:00:00Z", "endAt": "2026-03-25T10:30:00Z", "courtId": "...", "available": true },
    { "startAt": "2026-03-25T10:45:00Z", "endAt": "2026-03-25T12:15:00Z", "courtId": "...", "available": false }
  ]
}
```

### Création du match

**Appel API** :
```
POST /api/v1/matches
```

**Payload** :
```json
{
  "siteId": "uuid",
  "courtId": "uuid",
  "startAt": "2026-03-25T09:00:00Z",
  "type": "public",
  "privateParticipantsMatricules": []
}
```

Pour un **match privé** avec participants initiaux :
```json
{
  "type": "private",
  "privateParticipantsMatricules": ["G5678", "S0012"]
}
```

**Réponse** : `201 Created`
```json
{ "matchId": "uuid" }
```

### Règles métier appliquées automatiquement

| Règle | Code erreur retourné |
|-------|---------------------|
| Créneau déjà réservé | `409 booking.slot_conflict` |
| Fenêtre de réservation dépassée | `403 booking.reservation_window_denied` |
| Membre site hors de son site | `403 booking.site_scope_violation` |
| Organisateur avec dette active | `403 billing.organizer_debt_block` |

> **Spec (P1 §2, §4)** : Toutes ces règles sont évaluées dans `CreateMatchCommandHandler` avant tout accès à la base de données.

---

## 6. Rejoindre un match public

### Parcours utilisateur

```
[Page Rejoindre]
  → Voir la liste des matchs publics disponibles
  → Sélectionner un match
  → Rejoindre (paiement immédiat)
```

### Frontend

- **Route** : `/join`
- **Composant** : `JoinHomeComponent`
- **Appel** : `GET /api/v1/matches/public` ou `GET /api/v1/matches?scope=public`

### Rejoindre un match

**Appel API** :
```
POST /api/v1/matches/{matchId}/join
```

**Payload** :
```json
{ "idempotencyKey": "uuid-unique-par-tentative" }
```

> L'`idempotencyKey` garantit qu'une double soumission (ex: double-clic) ne génère pas deux paiements. Si la clé existe déjà, la réponse retourne le paiement existant sans créer de doublon.

**Réponse** : `200 OK`
```json
{ "paymentId": "uuid", "status": "paid" }
```

### Règles métier

| Règle | Code erreur |
|-------|------------|
| Match non public (privé, verrouillé, etc.) | `403 booking.match_not_public` |
| Match complet (4 participants) | `409 match.full` |
| Conflit de clé d'idempotence | `409 payment.idempotency_conflict` |

---

## 7. Mes matchs

### Frontend

- **Route** : `/matches`
- **Composants** : `MyMatchesComponent` → `MyMatchesPageComponent` → `MatchCardComponent`

### Filtres disponibles

| Filtre | Description |
|--------|-------------|
| Tous | Tous les matchs de l'utilisateur |
| À venir | Matchs dont `startAt > maintenant` |
| Passés | Matchs dont `startAt <= maintenant` |
| Public | Matchs de type public |
| Privé | Matchs de type privé |

### Appels API utilisés

```
GET /api/v1/matches?scope=mine
GET /api/v1/matches/user          ← endpoint legacy maintenu
GET /api/v1/sites                 ← pour résoudre les noms de sites
```

### Détail d'un match

```
GET /api/v1/matches/{matchId}
```

**Réponse** :
```json
{
  "matchId": "uuid",
  "siteId": "uuid",
  "courtId": "uuid",
  "startAtUtc": "2026-03-25T09:00:00Z",
  "endAtUtc": "2026-03-25T10:30:00Z",
  "type": "private",
  "status": "private",
  "organizerMemberId": "uuid",
  "priceTotalCents": 6000,
  "participants": [
    { "memberId": "uuid", "matricule": "G1234", "role": "organizer", "paymentStatus": "paid" },
    { "memberId": "uuid", "matricule": "S0012", "role": "player", "paymentStatus": "unpaid" }
  ]
}
```

> **Autorisation** : Un match **privé** n'est visible qu'aux participants et aux admins concernés. Toute autre demande reçoit `404` (et non `403`) pour ne pas révéler l'existence du match.

### Statuts d'un match

```
draft → private/public → full → locked → completed
                ↘ cancelled (admin ou organisateur avant lock)
```

| Statut | Signification |
|--------|--------------|
| `draft` | Transitoire à la création |
| `private` | Match privé en cours de remplissage |
| `public` | Match ouvert aux inscriptions |
| `full` | 4 participants payés |
| `locked` | Heure de début atteinte, plus de modifications |
| `completed` | Match terminé |
| `cancelled` | Annulé |

---

## 8. Paiements

### Paiement d'un match privé

Un participant d'un match privé doit payer **avant J-1** pour conserver sa place.

**Appel API** :
```
POST /api/v1/payments/matches/{matchId}/pay
```

**Payload** :
```json
{ "idempotencyKey": "uuid" }
```

**Réponse** : `200 OK`
```json
{ "paymentId": "uuid", "status": "paid" }
```

### Consulter un paiement

**Appel API** :
```
GET /api/v1/payments/{paymentId}
```

**Réponse** :
```json
{
  "paymentId": "uuid",
  "matchId": "uuid",
  "memberId": "uuid",
  "amountCents": 1500,
  "status": "paid",
  "createdAtUtc": "2026-03-24T14:32:00Z"
}
```

> **Autorisation** : Accessible uniquement au propriétaire du paiement ou à un administrateur. Retourne `404` si non autorisé.

### Prix

- Prix total d'un match : **60,00 €** (6 000 centimes)
- Prix par participant : **15,00 €** (1 500 centimes)

### Statuts d'un paiement

| Statut | Signification |
|--------|--------------|
| `pending` | En attente de traitement |
| `paid` | Paiement validé |
| `failed` | Paiement refusé |

> En environnement de démonstration, tous les paiements sont automatiquement acceptés (`paid`).

---

## 9. Cycle de vie automatique des matchs

Un background job (`MatchLifecycleJob`) s'exécute **toutes les minutes** et gère trois transitions automatiques.

### 9.1 Traitement J-1 (veille du match)

**Déclenchement** : la veille du match à partir de minuit UTC.

**Actions** :
1. Pour chaque match **privé** prévu le lendemain :
   - Les participants avec statut `unpaid` sont **exclus**.
   - Si des places sont libérées, le match **passe en public** (`private → public`).

> **Spec (P1 §3)** : Transition `private → public` déclenchée par J-1 si `participants < 4`.

### 9.2 Verrouillage au démarrage

**Déclenchement** : quand `startAt <= maintenant`.

**Actions** :
- Le match passe à `locked`.
- Si le match est incomplet (< 4 participants payés), un événement `MatchIncompleteEvent` est levé → une **dette** est créée ou augmentée pour l'organisateur.

### 9.3 Complétion à la fin

**Déclenchement** : quand `endAt <= maintenant`.

**Actions** :
- Le match passe à `completed`.

### Conséquence de la dette

Dès qu'un organisateur a une dette (`amountCents > 0`) :
- La création de tout nouveau match est **bloquée** (`403 billing.organizer_debt_block`).
- La dette est visible dans la vue opérationnelle admin.

---

## 10. Administration — Sites et terrains

### Accès

- **Route frontend** : `/admin`
- **Rôle requis** : `admin_site` ou `admin_global`

### Gestion des sites

| Action | Endpoint | Notes |
|--------|---------|-------|
| Lister les sites | `GET /api/v1/sites` | Filtrés par scope admin site |
| Détail d'un site | `GET /api/v1/sites/{siteId}` | |
| Créer un site | `POST /api/v1/sites` | `admin_global` uniquement |
| Modifier un site | `PUT /api/v1/sites/{siteId}` | |
| Activer | `POST /api/v1/sites/{siteId}/activate` | |
| Désactiver | `POST /api/v1/sites/{siteId}/deactivate` | |
| Supprimer | `DELETE /api/v1/sites/{siteId}` | Bloqué si réservations actives |

### Gestion des terrains

| Action | Endpoint |
|--------|---------|
| Lister | `GET /api/v1/sites/{siteId}/courts` |
| Détail | `GET /api/v1/sites/{siteId}/courts/{courtId}` |
| Créer | `POST /api/v1/sites/{siteId}/courts` |
| Modifier | `PUT /api/v1/sites/{siteId}/courts/{courtId}` |
| Supprimer | `DELETE /api/v1/sites/{siteId}/courts/{courtId}` |

### Gestion des horaires

Les horaires définissent les créneaux disponibles à la réservation. Un site peut avoir plusieurs plages horaires avec des **priorités** (une plage haute priorité écrase une basse priorité pour une même période).

| Action | Endpoint |
|--------|---------|
| Lister | `GET /api/v1/sites/{siteId}/schedules` |
| Créer | `POST /api/v1/sites/{siteId}/schedules` |
| Modifier | `PUT /api/v1/sites/{siteId}/schedules/{scheduleId}` |
| Supprimer | `DELETE /api/v1/sites/{siteId}/schedules/{scheduleId}` |

### Gestion des fermetures

| Action | Endpoint |
|--------|---------|
| Ajouter | `POST /api/v1/sites/{siteId}/closures` |
| Supprimer | `DELETE /api/v1/sites/{siteId}/closures/{closureId}` |

---

## 11. Administration — Vue opérationnelle

### Objectif

Donner à l'administrateur une vue en temps réel des situations nécessitant une attention immédiate sur un site.

### Accès

```
GET /api/v1/admin/sites/{siteId}/overview
```

- `admin_global` : accès à n'importe quel site
- `admin_site` : accès uniquement à son site (filtré côté serveur)

### Réponse

```json
{
  "siteId": "uuid",
  "alerts": [
    {
      "type": "j1_unprocessed",
      "description": "Private match scheduled for tomorrow has unpaid participants.",
      "payload": { "matchId": "uuid", "scheduledAt": "2026-03-26T09:00:00Z" }
    },
    {
      "type": "unpaid_participants",
      "description": "Match has 2 unpaid participant(s).",
      "payload": { "matchId": "uuid", "scheduledAt": "2026-03-27T14:00:00Z", "unpaidCount": 2 }
    },
    {
      "type": "organizer_debt",
      "description": "Organizer has an outstanding debt of 1500 cents.",
      "payload": { "memberId": "uuid", "amountCents": 1500 }
    }
  ]
}
```

### Types d'alertes

| Type | Déclenchement |
|------|--------------|
| `j1_unprocessed` | Match privé planifié demain avec des impayés |
| `unpaid_participants` | Match à venir (7 prochains jours) avec des participants non payés |
| `organizer_debt` | Tout organisateur avec une dette active (> 0 €) |

> **Spec (P4 §admin)** : Cet endpoint répond au besoin de "vue opérationnelle (alertes J-1, impayés, dettes)" décrit dans la spec.

---

## 12. Administration — Analytics revenus

### Objectif

Analyser le chiffre d'affaires généré par les paiements validés, par jour et par site.

### Accès

```
GET /api/v1/admin/analytics/revenue?siteId=&from=&to=
```

- `admin_global` : peut filtrer sur n'importe quel site, ou omettre `siteId` pour le global
- `admin_site` : le `siteId` est **toujours imposé côté serveur** sur son propre site, quelle que soit la valeur envoyée

### Paramètres

| Paramètre | Type | Requis | Description |
|-----------|------|--------|-------------|
| `siteId` | UUID | Non | Filtre sur un site (ignoré pour admin_site) |
| `from` | ISO 8601 | Oui | Début de la période |
| `to` | ISO 8601 | Oui | Fin de la période |

### Réponse

```json
{
  "from": "2026-01-01T00:00:00Z",
  "to": "2026-03-31T23:59:59Z",
  "currency": "EUR",
  "items": [
    { "date": "2026-01-15", "siteId": "uuid", "amountCents": 6000, "paymentCount": 4 },
    { "date": "2026-01-16", "siteId": "uuid", "amountCents": 4500, "paymentCount": 3 }
  ]
}
```

Les items sont regroupés par **jour** et par **site**, triés chronologiquement.

> **Source des données** : Uniquement les paiements avec `status = paid`. Les paiements `pending` ou `failed` ne sont pas comptabilisés.

---

## 13. Référence API

### Base URL

```
/api/v1
```

### Authentification

Toutes les requêtes nécessitent :
```
Authorization: Bearer <jwt>
```

### Endpoints complets

#### Identité
| Méthode | Chemin | Description | Rôle |
|---------|--------|-------------|------|
| GET | `/me` | Profil du membre connecté | Tout membre |

#### Disponibilités
| Méthode | Chemin | Description | Rôle |
|---------|--------|-------------|------|
| GET | `/availability` | Créneaux disponibles par site/date/terrain | Tout membre |

#### Matchs
| Méthode | Chemin | Description | Rôle |
|---------|--------|-------------|------|
| GET | `/matches` | Liste unifiée (`scope=public\|mine\|site`) | Tout membre (`scope=site` → admin) |
| GET | `/matches/public` | Matchs publics (endpoint legacy) | Tout membre |
| GET | `/matches/user` | Mes matchs (endpoint legacy) | Tout membre |
| GET | `/matches/{matchId}` | Détail d'un match | Tout membre |
| POST | `/matches` | Créer un match | Tout membre |
| POST | `/matches/{matchId}/join` | Rejoindre un match public | Tout membre |
| POST | `/matches/{matchId}/participants` | Ajouter un participant (privé) | Organisateur |
| POST | `/matches/{matchId}/cancel` | Annuler un match | Organisateur / Admin |

#### Paiements
| Méthode | Chemin | Description | Rôle |
|---------|--------|-------------|------|
| GET | `/payments/{paymentId}` | Détail d'un paiement | Owner / Admin |
| POST | `/payments/matches/{matchId}/pay` | Payer sa participation (privé) | Participant |

#### Sites
| Méthode | Chemin | Description | Rôle |
|---------|--------|-------------|------|
| GET | `/sites` | Liste des sites | Tout membre |
| GET | `/sites/{siteId}` | Détail d'un site | Tout membre |
| POST | `/sites` | Créer un site | `admin_global` |
| PUT | `/sites/{siteId}` | Modifier un site | Admin |
| DELETE | `/sites/{siteId}` | Supprimer un site | Admin |
| POST | `/sites/{siteId}/activate` | Activer | Admin |
| POST | `/sites/{siteId}/deactivate` | Désactiver | Admin |
| GET | `/sites/{siteId}/courts` | Terrains d'un site | Tout membre |
| GET | `/sites/{siteId}/courts/{courtId}` | Détail terrain | Tout membre |
| POST | `/sites/{siteId}/courts` | Créer un terrain | Admin |
| PUT | `/sites/{siteId}/courts/{courtId}` | Modifier un terrain | Admin |
| DELETE | `/sites/{siteId}/courts/{courtId}` | Supprimer un terrain | Admin |
| GET | `/sites/{siteId}/schedules` | Horaires | Admin |
| POST | `/sites/{siteId}/schedules` | Créer horaire | Admin |
| PUT | `/sites/{siteId}/schedules/{id}` | Modifier horaire | Admin |
| DELETE | `/sites/{siteId}/schedules/{id}` | Supprimer horaire | Admin |
| POST | `/sites/{siteId}/closures` | Ajouter fermeture | Admin |
| DELETE | `/sites/{siteId}/closures/{id}` | Supprimer fermeture | Admin |
| GET | `/sites/{siteId}/statistics` | Statistiques du site | Admin |

#### Admin
| Méthode | Chemin | Description | Rôle |
|---------|--------|-------------|------|
| GET | `/admin/sites/{siteId}/overview` | Alertes opérationnelles | Admin |
| GET | `/admin/analytics/revenue` | CA agrégé par période | Admin |

---

## 14. Codes d'erreur métier

Les erreurs sont retournées au format `application/problem+json` (RFC 7807).

```json
{
  "type": "booking.slot_conflict",
  "title": "This time slot is already booked.",
  "status": 409
}
```

### Catalogue des codes

| Code | HTTP | Signification |
|------|------|--------------|
| `booking.slot_conflict` | 409 | Créneau déjà réservé |
| `booking.reservation_window_denied` | 403 | Fenêtre de réservation dépassée |
| `booking.site_scope_violation` | 403 | Membre site hors de son site |
| `booking.match_not_found` | 404 | Match introuvable ou non autorisé |
| `booking.match_not_public` | 403 | Match non public |
| `booking.match_full` | 409 | Match complet (4 participants) |
| `booking.match_locked` | 409 | Match verrouillé |
| `booking.already_participant` | 409 | Déjà inscrit |
| `booking.not_participant` | 403 | Non inscrit au match |
| `booking.not_organizer` | 403 | Action réservée à l'organisateur |
| `billing.organizer_debt_block` | 403 | Création bloquée par une dette active |
| `billing.payment_not_found` | 404 | Paiement introuvable |
| `billing.idempotency_conflict` | 409 | Clé d'idempotence déjà utilisée |
| `billing.payment_already_processed` | 409 | Paiement déjà traité |
| `member.not_found` | 404 | Membre introuvable |
| `site.not_found` | 404 | Site introuvable |
| `court.not_found` | 404 | Terrain introuvable |

---

*Document généré à partir des specs P1 à P4 et de l'état d'implémentation au 25 mars 2026.*
