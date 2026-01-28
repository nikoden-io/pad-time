# P1 - Compréhension métier formalisée

---

# P 1 - Compréhension métier formalisée

Objectif : figer le **métier**, pas la technique

Livrables : base contractuelle du domaine (testable, non ambiguë)

---

## 1. Glossaire métier (`domain-glossary.md`)

### Site

- Unité de gestion regroupant terrains, horaires et fermetures.
- Périmètre d’autorité pour les admins site.

### Terrain

- Ressource physique unique.
- Appartient à un seul site.
- Réservable uniquement via un match.

### Créneau (Slot)

- Intervalle temporel fixe :
    - durée : 1h30
    - pause obligatoire : 15 min
- Défini par : terrain + start_at + end_at.
- Un slot ne peut contenir qu’un seul match.

### Match

- Réservation d’un terrain sur un créneau.
- Types : privé | public.
- Contient :
    - 1 organisateur
    - 0 à 3 autres participants
- Supporte des transitions d’état strictes.

### Organisateur

- Membre créateur du match.
- Responsable du remplissage et du paiement final.
- Peut accumuler une dette.

### Participant

- Membre inscrit à un match.
- Doit payer pour valider sa place.
- Statuts dépendants du paiement.

### Membre

- Utilisateur authentifié (OIDC).
- Identifiants :
    - `user_id` (technique)
    - `matricule` (métier)
- Catégories :
    - Global (Gxxxx)
    - Site (Sxxxxx)
    - Libre (Lxxxxx)

### Paiement (mock)

- Transaction simulée liée à un match + participant.
- Sert à valider une inscription ou solder une dette.

### Dette

- Montant dû par un organisateur.
- Bloque certaines actions tant qu’elle est > 0.

---

## 2. Règles métier formalisées (`business-rules.md`)

### Règles générales

- Un slot terrain ne peut être réservé qu’une seule fois.
- Un match contient au maximum 4 participants.
- Tout participant doit être membre valide.
- Toute action critique nécessite une identité authentifiée.

---

### Règles de réservation par catégorie

- **Global**
    - Peut créer un match à partir de J-21.
    - Tous sites autorisés.
- **Site**
    - Peut créer un match à partir de J-14.
    - Uniquement sur son site.
- **Libre**
    - Peut créer un match à partir de J-5.
    - Tous sites autorisés.

---

### Règles match privé

- Création :
    - type = privé
    - organisateur seul initialement
- Gestion joueurs :
    - ajout manuel par l’organisateur
- À J-1 :
    - si participants < 4 → match devient public
    - si paiement manquant → participant exclu
- Si match incomplet après J-1 :
    - pénalité appliquée
    - blocage réservation 7 jours pour l’organisateur

---

### Règles match public

- Création :
    - type = public
    - organisateur occupe 1 place
- Inscriptions :
    - paiement immédiat obligatoire
    - attribution des places atomique
- Si match incomplet :
    - organisateur paie le solde
    - dette créée ou augmentée
- Tant que dette > 0 :
    - création de match interdite
    - dette ajoutée à tout paiement ultérieur

---

### Règles de paiement

- Paiement requis avant J-1.
- Paiement non effectué :
    - place libérée
    - exclusion du participant
- Paiement idempotent (une action = un effet).

---

### Règles d’administration

- Admin global :
    - accès total tous sites.
- Admin site :
    - accès limité à son site.

---

## 3. Machines à états (`state-machines.md`)

### 3.1 Machine à états — Match

| État | Événement | État suivant |
| --- | --- | --- |
| draft | création | private/public |
| private | ajout participant | private |
| private | J-1 & < 4 joueurs | public |
| public | paiement participant | public/full |
| public | 4 joueurs payés | full |
| full | début créneau | locked |
| locked | fin match | completed |
| any | annulation admin | cancelled |

---

### 3.2 Machine à états — Paiement

| État | Événement | État suivant |
| --- | --- | --- |
| pending | paiement validé | paid |
| pending | paiement refusé | failed |
| paid | remboursement demandé | refunded |
| refunded | remboursement confirmé | refunded |

---

### 3.3 Machine à états — Dette organisateur

| État | Événement | État suivant |
| --- | --- | --- |
| none | match incomplet | open |
| open | paiement partiel | open |
| open | paiement complet | cleared |
| cleared | solde = 0 | none |

---

## 4. Invariants métier (non négociables)

- Double réservation d’un slot impossible.
- Dette > 0 ⇒ création de match interdite.
- Participant non payé ⇒ exclu.
- Transitions hors machine interdites.
- Les stats ne portent que sur états `completed`.

---

## 5. Indicateur de fin de Phase 1

- Glossaire exhaustif et partagé.
- Règles métier testables sans UI.
- États & transitions figés.
- Aucune décision métier ouverte.