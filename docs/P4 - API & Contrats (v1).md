# P4 - API & Contrats (v1)

---

# P4 - API & Contrats (v1)

Objectif : définir l’API **avant** implémentation (endpoints, payloads, erreurs, autorisations) + documenter le modèle de sécurité.

Livrables :

- `openapi.yaml` (v1, structurant, partiel acceptable)
- `security-model.md`

---

# 4.1 API v1 — Spécification fonctionnelle (sans code)

## Conventions globales

- Base path : `/api/v1`
- Auth : `Authorization: Bearer <jwt>`
- Format dates :
    - `date` : `YYYY-MM-DD`
    - `date-time` : ISO 8601
- Erreurs : `application/problem+json` (RFC7807)
- Pagination : `page` (>=1), `pageSize` (1..200)

## Codes HTTP standard

- `200` OK (lecture / action)
- `201` Created (création)
- `204` No Content (suppression logique / action sans contenu)
- `400` Validation / paramètres invalides
- `401` Token absent / invalide
- `403` Interdit (rôle, périmètre site, règle métier)
- `404` Non trouvé
- `409` Conflit (concurrence, slot déjà pris, match complet)

## Codes d’erreur métier (dans `problem.type` ou champ `code`)

- `booking.slot_conflict`
- `booking.reservation_window_denied`
- `booking.site_scope_violation`
- `billing.organizer_debt_block`
- `match.not_public`
- `match.full`
- `payment.idempotency_conflict`

---

## Endpoints — Identity / Session

### GET `/me`

But : projection des claims utiles (pour UI et audit).

- Auth : requis
- Réponse `200` :
    - `subject` (OIDC sub)
    - `matricule`
    - `category` : `global|site|free`
    - `role` : `user|admin_site|admin_global`
    - `siteId` (nullable)

---

## Endpoints — Référentiel (sites / terrains)

### GET `/sites`

But : lister les sites visibles (selon droits).

- Auth : requis
- Réponse `200` : liste `[{ siteId, name, timezone }]`

### GET `/sites/{siteId}/courts`

But : lister terrains d’un site.

- Auth : requis
- Réponse `200` : liste `[{ courtId, label, active }]`
- `403` si accès interdit (membre site hors périmètre si on choisit de filtrer strictement)

---

## Endpoints — Disponibilités

### GET `/availability?siteId=&date=&courtId=`

But : retourner les créneaux calculés (horaires annuels + fermetures) et la disponibilité (anti double-booking via matches existants).

- Auth : requis
- Query :
    - `siteId` (required)
    - `date` (required)
    - `courtId` (optional)
- Réponse `200` :
    - `siteId`, `date`, `slots[]`
    - slot :
        - `startAt`, `endAt`, `available`

---

## Endpoints — Matches (booking)

### POST `/matches`

But : créer un match privé/public.

- Auth : requis
- Autorisation :
    - `user` : ok si règles J-21/J-14/J-5 respectées + dette = 0 + scope site ok
    - `admin_*` : mêmes règles métier (sauf si on décide override admin, à documenter)
- Payload :
    - `siteId`
    - `courtId`
    - `startAt`
    - `type` : `private|public`
    - `privateParticipantsMatricules[]` (0..3) uniquement si `private`
- Réponses :
    - `201` `{ matchId }`
    - `409` si slot déjà pris (`booking.slot_conflict`)
    - `403` si dette (`billing.organizer_debt_block`) ou fenêtre réservation (`booking.reservation_window_denied`) ou périmètre site (`booking.site_scope_violation`)

### GET `/matches?scope=&siteId=&from=&to=&page=&pageSize=`

But : recherche / listing.

- Auth : requis
- `scope` :
    - `public` : matches publics visibles
    - `mine` : matches où je suis participant
    - `site` : matches d’un site (admin uniquement)
- Autorisation :
    - `scope=site` => `admin_site` (filtré site) ou `admin_global`
- Réponse `200` : pagination + `items[]` de `MatchDto`

### GET `/matches/{matchId}`

But : détail match.

- Auth : requis
- Autorisation :
    - public : visible à tous les membres
    - privé : visible uniquement participants + admin concerné
- Réponse `200` : `MatchDto`

### POST `/matches/{matchId}/join`

But : rejoindre un match public (paiement immédiat mock, idempotent).

- Auth : requis
- Payload :
    - `idempotencyKey` (UUID recommandé)
- Réponses :
    - `200` `{ paymentId, status }`
    - `409` si match complet (`match.full`) ou conflit idempotence (`payment.idempotency_conflict`)
    - `403` si match non public (`match.not_public`)

### POST `/matches/{matchId}/cancel`

But : annuler un match (MVP).

- Auth : requis
- Autorisation (MVP) :
    - `admin_global` : autorisé
    - `admin_site` : autorisé si match du site
    - `user` (organisateur) : autorisé uniquement si match non `locked`
- Réponses :
    - `204`
    - `409` si match `locked`

---

## Endpoints — Paiements (billing)

### GET `/payments/{paymentId}`

But : lire un paiement.

- Auth : requis
- Autorisation :
    - owner (le membre) ou admin concerné
- Réponse `200` : `PaymentDto`

---

## Endpoints — Admin (opérations)

### GET `/admin/sites/{siteId}/overview`

But : vue opérationnelle (alertes J-1, impayés, dettes) pour un site.

- Auth : requis
- Autorisation :
    - `admin_global` : ok
    - `admin_site` : ok uniquement si `siteId` = claim site
- Réponse `200` : `{ siteId, alerts[] }`

---

## Endpoints — Analytics (lecture agrégats)

### GET `/admin/analytics/revenue?siteId=&from=&to=`

But : CA agrégé (tables d’agrégats, pas de calcul lourd runtime).

- Auth : requis
- Autorisation :
    - `admin_global` : ok
    - `admin_site` : ok uniquement sur son site (siteId imposé/filtré serveur)
- Réponse `200` : `{ from, to, currency, items[] }`

---

# 4.2 Sécurité — Modèle et règles (`security-model.md`)

## OIDC / OAuth2

- Flow : Authorization Code + PKCE (web + mobile)
- Tokens :
    - access token JWT pour API
    - refresh token possible pour mobile (option)

## Scopes proposés

- `padel_api` : accès API standard (lecture/écriture user)
- `padel_admin` : endpoints admin
- `padel_analytics` : endpoints analytics admin

## Rôles

- `user`
- `admin_site`
- `admin_global`

## Claims minimales

- `sub` : identifiant technique
- `role`
- `matricule`
- `member_category` : `global|site|free`
- `site_id` : requis si `member_category=site` ou `role=admin_site`

## Règles d’accès (RBAC + ABAC)

- RBAC :
    - `admin_global` > tout
    - `admin_site` > admin site uniquement
    - `user` > endpoints non admin
- ABAC (site scope) :
    - `admin_site` limité à `site_id` du token
    - `member_category=site` limité à `site_id` pour création match
- L’API impose le filtrage serveur (jamais côté front)

## Matrice accès (résumé)

- `/me` : authenticated
- `/sites`, `/availability`, `/matches (GET)` : authenticated
- `/matches (POST)` : authenticated + règles métier + dette=0 + fenêtre réservations + scope site
- `/matches/{id}/join` : authenticated + match public + place dispo + idempotence
- `/admin/*` : `padel_admin` + (`admin_site`|`admin_global`)
- `/admin/analytics/*` : `padel_analytics` + (`admin_site`|`admin_global`)

---

# Annexes — Objets de réponse (DTO synthétiques)

## MatchDto

- `matchId`
- `siteId`
- `courtId`
- `startAt`, `endAt`
- `type` : private|public
- `status` : draft|private|public|full|locked|completed|cancelled
- `organizerMemberId`
- `participants[]` : { memberId, matricule, role, paymentStatus }
- `priceTotalCents` (= 6000)

## PaymentDto

- `paymentId`
- `matchId`
- `memberId`
- `amountCents`
- `status` : pending|paid|failed
- `createdAt`