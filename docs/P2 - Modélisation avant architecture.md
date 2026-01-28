# P2 - Modélisation avant architecture

---

# P2 - Modélisation avant architecture

Objectif : modéliser le **problème métier** avant toute décision technique.

---

## 3.1 Modèle conceptuel (ERD — conceptuel, sans techno)

> Modèle logique, indépendant de toute implémentation.
> 
> 
> Cardinalités et contraintes explicites.
> 

---

### Entités principales

## Site

- Identifiant
- Nom
- Fuseau horaire
- Actif / inactif

**Relations**

- 1 Site possède **1 à N Terrains**
- 1 Site possède **1 à N Règles horaires annuelles**
- 1 Site possède **0 à N Fermetures**

---

## Terrain

- Identifiant
- Libellé (ex: Court 1)
- Actif / inactif

**Relations**

- Appartient à **1 Site**
- Accueille **0 à N Matches**

**Contraintes**

- Un terrain appartient à un seul site
- Un terrain ne peut avoir qu’un match par créneau

---

## Règle horaire annuelle (SiteYearSchedule)

- Année civile
- Heure début journée
- Heure fin journée
- Durée créneau = 90 min
- Pause = 15 min

**Relations**

- Appartient à **1 Site**

**Contraintes**

- Une règle par site et par année
- Horaires valables uniquement pour l’année définie

---

## Fermeture

- Date
- Portée : globale ou site
- Motif

**Relations**

- Optionnellement liée à **1 Site**
- Peut être globale (sans site)

**Contraintes**

- Une date fermée ne génère aucun créneau réservable

---

## Membre

- Identifiant métier
- Matricule (Gxxxx / Sxxxxx / Lxxxxx)
- Catégorie : global / site / libre
- Site de rattachement (optionnel)
- Actif / inactif

**Relations**

- Peut organiser **0 à N Matches**
- Peut participer à **0 à N Matches**
- Peut avoir **0 ou 1 Dette**

**Contraintes**

- Matricule unique
- Catégorie cohérente avec le matricule
- Membre site obligatoirement rattaché à un site

---

## Match

- Identifiant
- Date et heure début
- Date et heure fin
- Type : privé / public
- Statut : draft / private / public / full / locked / completed / cancelled
- Organisateur

**Relations**

- Lié à **1 Site**
- Lié à **1 Terrain**
- Lié à **1 Organisateur**
- Contient **0 à 4 Participants**

**Contraintes**

- Un seul match par terrain et par créneau
- Maximum 4 participants
- Un seul organisateur

---

## Participant de match

- Rôle : organisateur / joueur
- Statut paiement : unpaid / pending / paid / failed / excluded
- Date d’inscription

**Relations**

- Appartient à **1 Match**
- Correspond à **1 Membre**

**Contraintes**

- Un membre ne peut apparaître qu’une fois par match
- Un seul organisateur par match

---

## Paiement (mock)

- Identifiant
- Montant
- Objet : part de match / dette
- Statut : pending / paid / failed
- Clé d’idempotence

**Relations**

- Lié à **1 Match**
- Lié à **1 Membre**

**Contraintes**

- Paiement idempotent
- Montant strictement positif

---

## Dette organisateur

- Montant dû
- Date dernière mise à jour

**Relations**

- Liée à **1 Membre**

**Contraintes**

- Montant >= 0
- Une seule dette par organisateur

---

## 3.2 Agrégats & responsabilités (Domain Model)

---

## Découpage par domaine (Bounded Contexts)

### Identity

- Authentification OpenID Connect
- Gestion des rôles et identités techniques
- Hors règles métier (aucune logique de réservation)

---

### Booking

- Gestion des créneaux
- Création et cycle de vie des matches
- Gestion des participants
- Application des règles J-21 / J-14 / J-5 / J-1
- Anti double-booking

**Agrégat principal**

- Match (racine)

---

### Billing

- Paiements (mock)
- Impayés
- Dettes organisateur
- Pénalités

**Agrégats**

- Payment
- OrganizerAccount (dette)

---

### Analytics

- Statistiques
- KPI
- Tableaux de bord
- Données dérivées uniquement

**Règle**

- Jamais source de vérité
- Lecture seule, pré-agrégée

---

## Agrégats et responsabilités

### Agrégat Match (Booking)

**Racine**

- Match

**Contrôle**

- Ajout / retrait participants
- Transitions d’état
- Respect du nombre maximum
- Bascule privé → public
- Verrouillage à l’heure du match

---

### Agrégat OrganizerAccount (Billing)

**Racine**

- Dette organisateur

**Contrôle**

- Création de dette
- Blocage réservation
- Réduction via paiements

---

### Agrégat Payment (Billing)

**Racine**

- Paiement

**Contrôle**

- Idempotence
- Validation ou échec
- Effets métier associés (place, dette)

---

## Transactionnel vs Analytique

### Transactionnel (source de vérité)

- Match
- Participants
- Paiements
- Dettes
- Règles horaires
- Fermetures

---

### Analytique (dérivé)

- Taux d’occupation
- Chiffre d’affaires
- Remplissage des matches
- Activité membres
- Heatmaps temporelles

---

## Décisions clés figées en Phase 2

- Le **booking** contrôle les créneaux et la concurrence
- Le **billing** contrôle paiements, impayés et dettes
- L’**identity** ne contient aucune règle métier
- L’**analytics** est découplé et non transactionnel

---

## Indicateur de fin de Phase 2

- Modèle conceptuel stable
- Responsabilités clairement séparées
- Aucun choix technique encore engagé
- Prêt pour la Phase 3 — Architecture cible