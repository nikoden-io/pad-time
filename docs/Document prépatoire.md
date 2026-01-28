# Document prépatoire

---

# Projet — Gestion de terrains de padel (spécification synthétique)

## Objectif

- Système de réservation multi-sites avec règles métiers strictes
- Priorité : réservations, paiements, pénalités, statistiques

## Périmètre fonctionnel

### Sites & terrains

- Plusieurs **sites**
- Nombre de **terrains variable par site**
- Horaires propres par site
    - créneaux : **1h30**
    - pause fixe : **15 min**
    - plage horaire définie **par année civile**
- Jours de fermeture
    - par site
    - globaux

### Matches

- **Privé** ou **public**
- Coût fixe : **60 € / match**
    - divisé en **4 joueurs**
    - paiement **obligatoire à l’avance**

### Match privé

- 4 joueurs requis
- Créateur ajoute les joueurs
- J-1 :
    - si < 4 joueurs → devient **public**
    - si paiement manquant → place libérée + match public
- Joueur non payé → exclu
- Pénalité si match incomplet :
    - **blocage réservation 1 semaine** pour le créateur

### Match public

- Créateur = organisateur
- 3 places ouvertes
- Inscription par **paiement immédiat**
- Règle : *premier payé = premier servi*
- Organisateur ne réserve pas pour autrui
- Si match incomplet :
    - organisateur paie le solde
    - dette bloquante pour toute nouvelle réservation
    - dette ajoutée à tout paiement futur

### Membres

- **Global (Gxxxx)**
    - réservation : **J-21**
    - tous sites
- **Site (Sxxxxx)**
    - réservation : **J-14**
    - uniquement son site
- **Libre (Lxxxxx)**
    - réservation : **J-5**
    - tous sites
- Tous visibles globalement
- Catégorie obligatoire pour chaque participant

### Authentification & identité

- **OpenID Connect** via **IdentityServer**
- Login requis (remplace “pas de login”)
- Matricule conservé comme identifiant métier (claim / attribut utilisateur)
- Rôles :
    - user
    - admin_site
    - admin_global

### Interfaces

- **Utilisateur**
    - réservations personnelles
    - matches publics
    - vue site / globale
- **Administrateur**
    - état terrains & matches
    - chiffre d’affaires
    - membres & statistiques
    - périmètre :
        - admin global → tous sites
        - admin site → site unique

## Stack technique imposée

### Frontend

- Web : **Angular** (OIDC client)
- Bonus mobile : **Flutter** (OIDC client)

### Backend

- API REST obligatoire
- **.NET**
- Intégration OIDC (JWT access tokens)
- Bonus IA : **Python (service séparé)**

### Base de données

- **PostgreSQL**
- Schéma relationnel strict
- DB dans container

## Architecture

- Séparation FE / BE / Identity
- Backend en couches :
    - controllers
    - services
    - repositories
    - modèles
- Injection de dépendances
- Clean architecture attendue

## CI / CD & DevOps

- **Docker** pour tous les composants (web, api, identity, db, optional ai)
- docker-compose pour l’environnement
- Tests automatiques **au push**
    - backend : controllers / services / repositories
- **GitFlow**
    - main / develop
    - feature/*
    - release/*
    - hotfix/*
- Gestion via issues Git

## Contraintes académiques

- Max **2 développeurs**
- Accès Git obligatoire
- Remise :
    - dépôt Git
    - analyse technique
- Démonstration :
    - session Teams 1h
    - exécution locale de la solution

---