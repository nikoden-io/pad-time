# P5 - Stratégie Data & Analytics

---

# P5 - Stratégie Data & Analytics

Rôle assumé : **Data Architect / Analytics Engineer senior**

Objectif : garantir des **statistiques fiables, performantes et démonstratives**, sans impacter le transactionnel.

---

## 1. Principes directeurs (non négociables)

- Les **tables transactionnelles ne servent jamais directement aux dashboards**
- Toute statistique visible repose sur des **agrégats pré-calculés**
- Les règles métier déterminent **quand** une donnée devient analytique
- Les agrégats sont :
    - déterministes
    - recalculables
    - traçables (source → KPI)

---

## 2. Séparation des données

### 2.1 Données transactionnelles (source de vérité)

Utilisées uniquement pour :

- exécuter le métier
- produire des événements

Tables concernées (rappel) :

- site, terrain, site_year_schedule, closure
- member
- match
- match_participant
- payment
- organizer_debt

Aucune requête analytique lourde dessus.

---

### 2.2 Données analytiques (schéma `analytics`)

- Lecture seule pour l’API
- Alimentées par :
    - événements métier
    - jobs batch / background
- Supprimables / recalculables sans perte métier

---

## 3. Événements métier produits

### 3.1 Pourquoi des événements

- Découpler métier et analytics
- Garantir cohérence temporelle
- Faciliter bonus IA (Python)

---

### 3.2 Liste des événements (canonique)

Chaque événement contient :

- `event_id`
- `event_type`
- `occurred_at`
- `site_id`
- `match_id` (si applicable)
- payload métier minimal

### Booking

- `match_created`
- `match_became_public`
- `match_full`
- `match_locked`
- `match_completed`
- `match_cancelled`

### Participants

- `participant_joined`
- `participant_paid`
- `participant_excluded`

### Billing

- `payment_succeeded`
- `payment_failed`
- `organizer_debt_created`
- `organizer_debt_updated`
- `organizer_debt_cleared`

### Pénalités

- `penalty_applied`

---

### 3.3 Stockage événements (MVP)

- Table `domain_event_outbox`
- Écrite dans la même transaction que le métier
- Consommée par :
    - job analytics interne (.NET)
    - service IA (bonus)

---

## 4. Tables d’agrégats (analytics)

### 4.1 Occupation des terrains

### `analytics.occupancy_daily`

| Champ | Description |
| --- | --- |
| date | jour |
| site_id | site |
| court_id | terrain |
| slots_total | slots théoriques |
| slots_booked | slots réservés |
| occupancy_rate | slots_booked / slots_total |

---

### 4.2 Chiffre d’affaires

### `analytics.revenue_daily`

| Champ | Description |
| --- | --- |
| date | jour |
| site_id | site |
| revenue_cents | total encaissé |
| unpaid_cents | impayés |
| matches_completed | volume |

---

### 4.3 Remplissage des matches publics (funnel)

### `analytics.public_match_funnel`

| Champ | Description |
| --- | --- |
| week | semaine ISO |
| site_id | site |
| created_public | créés |
| reached_1 | ≥1 joueur |
| reached_2 | ≥2 joueurs |
| reached_3 | ≥3 joueurs |
| full | 4 joueurs |

---

### 4.4 Dettes & pénalités

### `analytics.organizer_debt_snapshot`

| Champ | Description |
| --- | --- |
| date | jour |
| site_id | site |
| organizers_with_debt | nombre |
| total_debt_cents | montant |
| avg_debt_cents | moyenne |

---

### 4.5 Activité membres

### `analytics.member_activity_monthly`

| Champ | Description |
| --- | --- |
| month | AAAA-MM |
| member_id | membre |
| site_id | site |
| matches_played | volume |
| spent_cents | total payé |
| penalties_count | pénalités |

---

## 5. KPI exposés (contractuels)

### 5.1 KPI opérationnels (admin site)

- Taux d’occupation par jour / heure
- Matches à risque J-1 (incomplets)
- Nombre d’impayés
- Dettes actives
- Temps moyen de remplissage match public

---

### 5.2 KPI business (admin global)

- CA journalier / hebdomadaire / mensuel
- CA par site
- CA par type (public / privé)
- Panier moyen par match
- Récurrence membres (mensuelle)

---

### 5.3 KPI qualité système (démo technique)

- % bascules privé → public
- % matches incomplets
- % matchs remplis avant J-1
- % revenus issus matches publics
- Délai moyen paiement → confirmation place

---

## 6. Dashboards attendus (Angular)

### 6.1 Dashboard utilisateur

- Calendrier personnel
- Historique paiements
- Badge dette / pénalité
- Indicateur fiabilité (matches joués vs annulés)

---

### 6.2 Dashboard admin site

- Heatmap occupation (jour × heure)
- Liste matches à risque J-1
- Funnel matches publics
- CA site (timeline)
- Dettes actives + alertes

---

### 6.3 Dashboard admin global

- Comparatif sites (occupation / CA)
- Évolution mensuelle CA
- Top sites / top membres
- Vue consolidée pénalités

---

## 7. Fréquence de calcul

- Occupation : quotidien (batch)
- CA : quasi temps réel (event-driven)
- Funnels : hebdomadaire
- Activité membres : mensuelle
- Snapshots dettes : quotidien

---

## 8. API Analytics (rappel)

- L’API **ne calcule rien**
- Elle lit uniquement :
    - tables `analytics.*`
- Filtrage par site appliqué serveur (ABAC)

---

## 9. Bonus IA (optionnel, futur)

Le service Python peut :

- prédire taux d’occupation futur
- estimer CA à J+7 / J+30
- suggérer ouverture de créneaux
- détecter membres à risque (dettes récurrentes)

Entrée : événements + agrégats

Sortie : tables `analytics.predictions_*`

---

## 10. Indicateur de fin Phase 5

- Chaque KPI est traçable à :
    - 1 ou plusieurs événements
    - 1 table d’agrégat
- Aucun dashboard ne dépend du transactionnel
- Les stats sont explicables et défendables à l’oral