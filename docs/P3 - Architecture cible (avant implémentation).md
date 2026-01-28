# P3 - Architecture cible (avant implémentation)

---

# Phase 3 - Architecture cible (avant implémentation)

Objectif : figer la **structure** (composants, flux, responsabilités, contraintes) avant d’écrire du code.

---

## 3.1 Vue d’ensemble (C4 — niveau Container)

### Containers

- **Web App (Angular)**
    - UI user + UI admin
    - OIDC client (Authorization Code + PKCE)
    - Appels API via access token (JWT)
- **Mobile App (Flutter) — bonus**
    - OIDC client (Authorization Code + PKCE)
    - Même API
- **Identity Provider (IdentityServer)**
    - OpenID Connect / OAuth2
    - Gestion utilisateurs + rôles
    - Émission tokens (access token JWT)
    - Expose : discovery document, authorize, token, userinfo (si requis)
- **Backend API (.NET)**
    - REST API
    - Autorisation (RBAC + ABAC par site)
    - Domaines : Booking + Billing + Analytics (lecture)
    - Background jobs (J-1 bascules, impayés, calculs agrégats)
- **Database (PostgreSQL)**
    - Données transactionnelles (booking/billing)
    - Données analytiques (agrégats) séparées logiquement (schéma/table)
- **AI Service (Python) — bonus**
    - Lecture agrégats + événements (option)
    - Produit prédictions (forecast, recommandations)

---

## 3.2 Flux principaux

### Authentification (Angular / Flutter)

1. Client → IdentityServer : authorize (PKCE)
2. IdentityServer → Client : code
3. Client → IdentityServer : token (code + verifier)
4. IdentityServer → Client : access_token (+ id_token)
5. Client → API : Authorization: Bearer <access_token>
6. API :
    - valide signature + audience + expiry
    - applique policies (roles + site scope)

---

### Booking (création match)

1. Client → API : create match (site, terrain, slot, type)
2. API :
    - vérifie droit de réservation (catégorie + fenêtre J-21/J-14/J-5)
    - vérifie dette organisateur = 0
    - vérifie fermetures + horaires
    - écrit Match + participant organizer
    - protège la concurrence (unique slot)
3. API → Client : match créé

---

### Public join + paiement (premier payé = premier servi)

1. Client → API : init payment (idempotency key)
2. API :
    - transaction courte
    - réserve place si disponible (atomique)
    - crée Payment pending
3. API : simule/valide paiement → Payment paid
4. API :
    - marque participant paid
    - si 4 payés → match full
5. API → Client : confirmation

---

### Automatisations J-1 (background job)

- Exécution périodique (cron interne)
- Pour chaque match concerné :
    - privé incomplet → devient public
    - impayés → exclusion + libération place
    - pénalité si incomplet (selon règle métier)
- Génère événements (ou écrit directement agrégats)

---

### Analytics (dashboards)

- API expose endpoints dédiés lecture :
    - occupation (heatmap)
    - CA (jour/semaine/mois)
    - fill-rate matches publics
    - dettes/pénalités
- Les données proviennent de tables d’agrégats (pas de calcul lourd runtime)

---

## 3.3 Architecture interne Backend (.NET)

### Style

- Clean / layered architecture
- Modules logiques :
    - Booking
    - Billing
    - Analytics (read models)
    - Identity integration (authZ only)

### Couches (exigées)

- API layer : controllers + contracts (DTO)
- Application layer : services / use cases
- Domain layer : entités + règles + invariants
- Infrastructure layer : persistence + identity integration + jobs

### Règles

- Le Domain ne dépend de rien.
- L’Application orchestre (transactions, appels repos).
- L’Infrastructure implémente (EF/Npgsql, outbox éventuelle, clock, etc).

---

## 3.4 Autorisation (RBAC + ABAC)

### Rôles (IdentityServer)

- `user`
- `admin_site`
- `admin_global`

### Claims métier minimales

- `sub` (technique)
- `matricule`
- `member_category` (global/site/free)
- `site_id` (uniquement pour member_category=site)
- `role`

### Policies API

- `admin_global` : accès total
- `admin_site` : accès endpoints admin + filtrage obligatoire sur `site_id`
- `user` : accès user + création match selon règles

**Règle non négociable**

- L’API ne se fie jamais au front pour restreindre le périmètre site.

---

## 3.5 Données (transactionnel vs analytique)

### Transactionnel (source de vérité)

- sites, terrains, horaires, fermetures
- members (profil métier)
- matches, participants
- payments
- debts

### Analytique (dérivé)

- tables d’agrégats (par jour/semaine, par site/terrain)
- destinées au dashboard
- recalculables

**Séparation recommandée**

- Schéma `public` : transactionnel
- Schéma `analytics` : agrégats

---

## 3.6 Concurrence & robustesse (décisions)

### Anti double booking

- contrainte unique : (court_id, start_at)
- transaction de création match courte

### Premier payé = premier servi

- join public basé sur update/insert atomique
- limitation à 4 participants actifs
- idempotence via `idempotency_key` sur Payment

### Idempotence

- création paiement et confirmation place : toujours idempotent

---

## 3.7 Background jobs (décisions)

- Dans l’API .NET :
    - service de fond (hosted service) + planification simple
- Jobs MVP :
    - J-1 bascule privé→public
    - exclusion impayés
    - création/augmentation dettes et pénalités
    - mise à jour agrégats stats

---

## 3.8 Déploiement local (Docker)

### docker-compose (MVP)

- postgres
- identityserver
- api
- web

### docker-compose (bonus)

- 
    - ai-service (python)
- 
    - mobile non containerisé (build local)

---

## 3.9 CI au push (GitFlow)

### Branches

- main : release stable
- develop : intégration
- feature/* : dev
- release/* : stabilisation
- hotfix/* : correctifs prod

### Pipeline minimal

- lint/build Angular + tests
- build .NET + unit tests
- integration tests .NET avec postgres (service container)
- build images docker (optionnel en CI)

---

## 3.10 Décisions d’architecture (ADR light)

- OIDC via IdentityServer (tokens JWT) obligatoire
- PostgreSQL unique source de vérité
- Stats via agrégats (pas de requêtes lourdes runtime)
- Contrainte DB + transactions pour concurrence
- Background jobs dans API (MVP)

---

## 3.11 Critère de fin de Phase 3

- Diagramme containers figé
- Flux auth + booking + billing décrits
- Policies d’accès définies
- Stratégie stats validée (agrégats)
- Stratégie concurrence validée (contraintes + idempotence)
- Prêt pour Phase 4 : Contrats API (OpenAPI) + modèle de sécurité détaillé