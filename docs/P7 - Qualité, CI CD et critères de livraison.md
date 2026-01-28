# P7 - Qualité, CI/CD et critères de livraison

---

# P7 - Qualité, CI/CD et critères de livraison

Rôle assumé : **DevOps Lead / Quality Engineer senior**

Objectif : garantir build reproductible, tests automatiques, qualité mesurable, démonstration fluide.

---

## 1. Definition of Done (DoD) — applicable à chaque PR

### Backend (.NET)

- Tests unitaires ajoutés/ajustés (services + domaine)
- Tests d’intégration si accès DB / contraintes / concurrence
- Couverture minimale (cible) : 70% services/domain (indicatif)
- Analyse statique activée (warnings traités)
- Endpoints documentés (OpenAPI cohérent)

### Frontend (Angular)

- Build OK + lint OK
- Tests unitaires composants/services (cible : flux critiques)
- Aucun appel API sans gestion erreurs

### Infra

- `docker-compose up` démarre sans intervention manuelle
- Variables d’environnement documentées
- Migrations exécutées automatiquement au démarrage API (dev)

### Général

- Observabilité minimale : logs structurés + correlation id
- Pas de secrets en dur dans le repo
- Changelog/notes release (si release/*)

---

## 2. Stratégie de tests

### 2.1 Backend — pyramide

- Unit (rapides)
    - règles métier : fenêtres J-21/J-14/J-5, dette, max 4, transitions match
- Integration (DB)
    - contraintes uniques anti double-booking
    - join match public concurrence (tests multi-thread / transactions)
    - idempotence paiement
- API contract smoke (optionnel)
    - endpoints essentiels répondent (200/201/401/403/409)

### 2.2 Frontend

- Unit :
    - services API (mapping DTO)
    - guards OIDC / rôle
- E2E (optionnel) :
    - login OIDC + réservation + join public

---

## 3. Observabilité minimale (démo + prod-like)

- Header `X-Correlation-Id`
    - généré si absent
    - propagé dans logs
- Logs structurés
    - inclure `sub`, `matricule`, `site_id` (si présent)
- Health endpoints
    - `/health/live`
    - `/health/ready` (db + deps)

---

## 4. Containerisation (Docker)

### 4.1 Images (MVP)

- `web` : Angular (build -> static) servi par nginx (ou node minimal)
- `api` : .NET (multi-stage)
- `identity` : IdentityServer (multi-stage)
- `db` : postgres officielle

### 4.2 docker-compose (dev)

- Réseau unique
- Volumes :
    - postgres data
- Variables via `.env` (non commité)

### 4.3 Configuration runtime (principes)

- API :
    - connection string via env
    - authority OIDC via env
- Web :
    - API base URL + authority via env / config injectée

---

## 5. GitFlow — règles de branches

- `main` : releases stables
- `develop` : intégration
- `feature/*` : dev
- `release/*` : stabilisation
- `hotfix/*` : correction main

Règles PR (recommandé)

- PR obligatoire vers `develop`
- CI obligatoire verte avant merge
- squash merge ou merge commit (à choisir et figer)

---

## 6. CI au push (pipeline minimal)

### Déclencheurs

- Push sur `feature/*` et `develop`
- PR vers `develop`
- Push sur `release/*` et `main` (inclut build images)

### Jobs (ordre logique)

1. **lint + build Angular**
2. **test Angular**
3. **build .NET**
4. **tests unit .NET**
5. **tests intégration .NET avec postgres service**
6. (optionnel) **build docker images**

Artifacts

- rapport tests
- (optionnel) couverture

---

## 7. Exemple pipeline (GitHub Actions — structure)

Fichier : `.github/workflows/ci.yml`

Étapes recommandées :

- checkout
- setup node + cache npm
- npm ci / test / build (web)
- setup dotnet + cache nuget
- dotnet test (unit)
- démarrer postgres service (container) + dotnet test (integration)
- publier résultats

---

## 8. Environnements & secrets

### Dev

- `.env` local
- secrets factices pour demo

### CI

- secrets dans GitHub Secrets
- aucun secret en clair dans YAML

Variables typiques

- `postgres__connectionstring`
- `oidc__authority`
- `oidc__audience`
- `identity__issuer`
- `identity__signingkey` (dev only)

---

## 9. Critères de livraison (démonstration examen)

Checklist démo

- `docker-compose up` :
    - identity accessible
    - api ready
    - web up
- Scénarios démonstrables (ordre)
    1. login
    2. availability
    3. create match privé/public
    4. join public (idempotence visible)
    5. dette bloque création
    6. admin overview + revenue

---

## 10. Indicateur de fin Phase 7

- CI verte sur develop
- Tests couvrent règles critiques + concurrence
- docker-compose reproductible
- health + logs + correlation id opérationnels
- prêt pour Phase 8 : implémentation MVP par epics