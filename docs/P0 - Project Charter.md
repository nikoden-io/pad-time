# P0 - Project Charter

---

# P0 - Project Charter — Gestion de terrains de padel

## Vision

Construire une plateforme **professionnelle, robuste et démonstrative** de réservation de terrains de padel multi-sites, intégrant règles métiers complexes, paiements, statistiques avancées et une architecture moderne (OIDC, CI/CD, Docker).

Objectif principal : **démontrer une maîtrise complète d’un projet from-scratch**, du cadrage à l’exécution.

---

## Portée du projet

### MVP obligatoire (non négociable)

### Fonctionnel

- Gestion multi-sites / multi-terrains
- Création de matches privés et publics
- Règles J-21 / J-14 / J-5 selon type de membre
- Bascule automatique privé → public à J-1
- Paiement obligatoire (mock) + gestion des impayés
- Dettes organisateur bloquantes
- Catégories membres (global / site / libre)
- Interfaces :
    - utilisateur (réservations, matches publics)
    - administrateur (état, CA basique)

### Technique

- Front web Angular
- Backend .NET REST API
- Authentification OpenID Connect (IdentityServer)
- PostgreSQL relationnel
- Docker + docker-compose
- Tests backend (services + repos minimum)
- GitFlow + CI au push

### Bonus démonstratif (si temps)

- Front mobile Flutter
- Service IA Python (prévisions / recommandations)
- Dashboards avancés :
    - heatmaps d’occupation
    - funnel remplissage matches publics
    - statistiques comportementales membres
- Agrégats analytics pré-calculés
- Observabilité avancée (metrics, traces)
- Export CSV/PDF admin

### Hors scope (exclu explicitement)

- Paiement réel (PSP, cartes bancaires)
- Gestion comptable ou TVA
- Gestion matériel / abonnements
- Notifications email / SMS
- Multilingue
- Gestion légale / RGPD avancée
- Scalabilité cloud native (K8s, autoscaling)

## Critères de réussite

### Fonctionnels (scénarios clés)

- Un slot terrain ne peut jamais être réservé deux fois
- Un match public applique strictement : *premier payé = premier servi*
- Un organisateur avec dette ne peut pas créer de match
- À J-1, les règles de bascule sont appliquées automatiquement
- Un admin site ne voit et ne gère que son site
- Les statistiques affichées correspondent aux données réelles

### Techniques

- Build complet via `docker-compose up`
- Tests backend exécutés automatiquement en CI
- API sécurisée par JWT (OIDC)
- Séparation claire :
    - Identity
    - API
    - Front
    - DB
- Modèle DB avec contraintes fortes (FK, uniques)
- Code structuré (controllers / services / repositories)
- Documentation technique minimale mais claire

## Contraintes

- Équipe : **1–2 développeurs max**
- Temps limité (projet académique)
- Démonstration live obligatoire
- Clarté et robustesse > exhaustivité
- Toute règle métier doit être traçable (doc → code)

## Risques majeurs identifiés

- Explosion de complexité métier non cadrée
- Concurrence (double booking, paiements simultanés)
- Surinvestissement dans les bonus au détriment du MVP
- Stats calculées en temps réel → performances faibles
- Couplage excessif auth / métier

Mesures :

- MVP figé avant développement
- Contraintes DB + transactions dès le départ
- Stats via agrégats
- Bonus uniquement après MVP validé

## Décisions irréversibles (figées)

- **Frontend web** : Angular
- **Backend** : .NET REST API
- **Authentification** : OpenID Connect via IdentityServer
- **Base de données** : PostgreSQL
- **Conteneurisation** : Docker pour tous les composants
- **CI/CD** : tests automatiques au push
- **Workflow Git** : GitFlow
- **Architecture** : Clean / layered architecture
- **Paiement** : mock transactionnel (pas réel)

## Indicateur de passage à la phase suivante

- MVP clairement compris et accepté
- Hors scope assumé
- Risques identifiés et maîtrisés
- Aucune décision structurante en suspens