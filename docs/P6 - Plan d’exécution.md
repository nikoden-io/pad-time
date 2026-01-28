# P6 - Plan d’exécution

---

# Phase 6 - Plan d’exécution

Rôle assumé : **Delivery Lead / Tech Lead senior**

Objectif : transformer l’analyse en **exécution maîtrisée**, sans dette organisationnelle.

---

## 7.1 Backlog structuré (`backlog.md`)

### Principes de découpage

- Découpage par **valeur métier livrable**
- Respect strict des **dépendances techniques**
- Chaque item :
    - testable
    - démontrable
    - intégrable sans refonte

---

## EPIC 1 — Fondations data & règles cœur (bloquant)

### US 1.1 — Modèle de données transactionnel

- Création schéma PostgreSQL
- Tables + contraintes :
    - sites, terrains
    - horaires annuels, fermetures
    - members (profil métier)
    - matches, participants
    - payments
    - organizer_debt
- Contraintes uniques / FK / checks
- Migrations versionnées

**Valeur**

- Garantit intégrité et concurrence
- Base de tout le reste

**Dépendances**

- Aucune

---

### US 1.2 — Règles métier cœur (domaine)

- Fenêtres J-21 / J-14 / J-5
- Dette bloquante
- Max 4 participants
- Anti double-booking
- Transitions d’état match (P1)

**Valeur**

- Sécurise le métier
- Rend le système prédictible

**Dépendances**

- US 1.1

---

## EPIC 2 — API Booking (réservations)

### US 2.1 — Disponibilités (slots)

- Calcul créneaux via horaires + fermetures
- Marquage disponibilité (match existant)
- Endpoint `/availability`

**Valeur**

- Première fonctionnalité visible
- Pré-requis UX réservation

**Dépendances**

- EPIC 1

---

### US 2.2 — Création match

- Endpoint `POST /matches`
- Vérifications :
    - éligibilité membre
    - périmètre site
    - dette = 0
    - slot libre
- Création match + organisateur

**Valeur**

- Cœur du produit

**Dépendances**

- US 2.1

---

### US 2.3 — Listing / détail matches

- Endpoints `GET /matches`, `GET /matches/{id}`
- Scopes : public / mine / site
- Pagination

**Valeur**

- Consultation utilisateur et admin

**Dépendances**

- US 2.2

---

## EPIC 3 — Paiement mock & dettes (billing)

### US 3.1 — Paiement mock idempotent

- Endpoint `POST /matches/{id}/join`
- Gestion idempotency key
- Attribution place atomique
- États paiement

**Valeur**

- Sécurise “premier payé = premier servi”

**Dépendances**

- EPIC 2

---

### US 3.2 — Gestion dettes organisateur

- Création dette si match public incomplet
- Blocage création match si dette > 0
- Propagation dette sur paiements futurs

**Valeur**

- Règle métier différenciante
- Démonstration rigueur domaine

**Dépendances**

- US 3.1

---

### US 3.3 — Jobs J-1 (automatisations)

- Bascule privé → public
- Exclusion impayés
- Pénalité organisateur

**Valeur**

- Robustesse temps réel
- Cas d’examen clé

**Dépendances**

- EPIC 3

---

## EPIC 4 — Authentification & autorisation

### US 4.1 — IdentityServer (OIDC)

- Setup IdentityServer
- Clients Angular / Flutter
- Scopes / rôles / claims

**Valeur**

- Sécurité réelle
- Architecture professionnelle

**Dépendances**

- EPIC 1

---

### US 4.2 — Sécurité API

- Validation JWT
- Policies RBAC + ABAC
- Filtrage site serveur

**Valeur**

- Isolation multi-site
- Zéro confiance front

**Dépendances**

- US 4.1 + EPIC 2

---

## EPIC 5 — Analytics & dashboards

### US 5.1 — Événements métier (outbox)

- Table événements
- Émission transactionnelle
- Consommation batch

**Valeur**

- Base analytics + IA

**Dépendances**

- EPIC 2 + 3

---

### US 5.2 — Agrégats analytics

- occupancy_daily
- revenue_daily
- public_match_funnel
- organizer_debt_snapshot

**Valeur**

- Performance
- Lisibilité business

**Dépendances**

- US 5.1

---

### US 5.3 — Dashboards Angular

- Admin site
- Admin global
- Visualisations (heatmap, timelines)

**Valeur**

- Démonstration technique forte
- UX data-driven

**Dépendances**

- US 5.2

---

## EPIC 6 — Bonus démonstratifs (optionnel)

### US 6.1 — Service IA Python

- Lecture agrégats
- Prédictions occupation / CA
- Exposition résultats

### US 6.2 — App mobile Flutter

- Auth OIDC
- Parcours utilisateur minimal

---

## 7.2 Ordre de développement (recommandé)

1. **DB + règles cœur**
    - migrations
    - contraintes
    - domaine
2. **API réservation**
    - availability
    - create/list/get match
3. **Paiement mock + dettes**
    - join public
    - blocage dette
    - jobs J-1
4. **Auth + rôles**
    - IdentityServer
    - policies API
5. **Stats + dashboards**
    - événements
    - agrégats
    - UI admin
6. **Bonus IA / mobile**
    - uniquement si MVP validé

---

## Indicateur de fin Phase 6

- Backlog ordonné et priorisé
- Aucun item critique sans dépendance résolue
- MVP atteignable sans bonus
- Plan défendable face à un jury technique