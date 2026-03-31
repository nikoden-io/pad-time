# Scenarios de demo PadTime

> Mot de passe universel : `Passw0rd!`

---

## Comptes disponibles

| Email | Matricule | Cat. | Role | Particularite |
|-------|-----------|------|------|---------------|
| admin@test.be | G0001 | Global | admin_global | Acces dashboard admin |
| alice@test.be | G1001 | Global | user | Joueuse tres active |
| bob@test.be | G1002 | Global | user | 15 EUR de dette (1 match incomplet) |
| claire@test.be | G1003 | Global | user | |
| david@test.be | G1004 | Global | user | |
| helene@test.be | G1005 | Global | user | |
| kevin@test.be | G1006 | Global | user | |
| nathalie@test.be | G1007 | Global | user | |
| emma@test.be | S10001 | Site (BXL) | user | Ne peut reserver qu'a Bruxelles |
| francois@test.be | S10002 | Site (LGE) | user | Ne peut reserver qu'a Liege |
| ibrahim@test.be | S10003 | Site (BXL) | user | |
| lea@test.be | S10004 | Site (LGE) | user | |
| **georges@test.be** | **L10001** | **Free** | **user** | **45 EUR de dette - BLOQUE** |
| julie@test.be | L10002 | Free | user | |
| marc@test.be | L10003 | Free | user | |

---

## Scenario 1 : Dashboard admin et KPIs

**Connexion :** admin@test.be

1. Aller sur `/admin`
2. Verifier les 4 KPIs en haut :
   - **Sites actifs** : 2 (Brussels Padel Center + Liege Sport Complex)
   - **Matchs aujourd'hui** : 4 matchs completes ce jour
   - **CA du jour** : 240,00 EUR (4 matchs x 4 joueurs x 15 EUR)
   - **Dettes actives** : affiche les dettes en cours
3. Les 4 cartes navigation : Sites, Vue operationnelle, Analytics, Membres (coming soon)

**Point cle :** Montrer que les KPIs se mettent a jour en temps reel au fil des matchs de la journee.

---

## Scenario 2 : Vue operationnelle (alertes J-1)

**Connexion :** admin@test.be

1. Depuis le dashboard, cliquer sur **Vue operationnelle**
2. Selectionner **Brussels Padel Center**
3. Observer les alertes :
   - **Matchs J-1 non traites** : matchs de demain pas encore complets ou pas payes
   - **Participants impayes** : joueurs inscrits mais n'ayant pas encore paye
   - **Dettes organisateurs** : Georges (45 EUR) et Bob (15 EUR) ont des dettes
4. Selectionner **Liege Sport Complex** et observer les alertes specifiques a ce site
5. Montrer qu'un site dont tous les matchs sont payes/complets affiche "Aucune alerte"

**Point cle :** L'admin voit d'un coup d'oeil ce qui requiert son attention avant les matchs du lendemain.

---

## Scenario 3 : Analytics revenus

**Connexion :** admin@test.be

1. Depuis le dashboard, cliquer sur **Analytics**
2. La periode par defaut couvre les 30 derniers jours
3. Cliquer **Charger** : afficher le tableau des revenus
   - Voir le total revenus, nombre de paiements, jours actifs
   - Tableau jour par jour avec site, nombre de paiements, montant
4. Changer la periode : mettre les 6 derniers mois (ex: du 01/10/2025 au aujourd'hui)
   - Observer la courbe de revenus qui monte au fil des mois
5. Filtrer par site : selectionner **Brussels Padel Center** uniquement
   - Les revenus n'affichent que Bruxelles
6. Remettre "Tous les sites" et comparer

**Point cle :** ~80+ matchs passes generent des donnees realistes sur 6 mois.

---

## Scenario 4 : Gestion des sites

**Connexion :** admin@test.be

1. Depuis le dashboard, cliquer sur **Sites**
2. Voir la liste : Brussels Padel Center (4 terrains) et Liege Sport Complex (3 terrains)
3. Cliquer sur **Voir Details** d'un site
4. Explorer les onglets :
   - **Terrains** : liste des courts (C1, C2, C3, C4)
   - **Horaires** : 08h-22h (BXL) / 09h-21h (LGE)
   - **Fermetures** : jours feries belges 2025-2026
5. Activer/desactiver un site et observer l'impact sur les KPIs

---

## Scenario 5 : Joueur bloque par ses dettes (Georges)

**Connexion :** georges@test.be

1. Se connecter en tant que Georges Peeters (L10001, Free)
2. Tenter de **creer un nouveau match**
3. **Resultat attendu :** Georges est bloque car il a 45 EUR de dette (3 matchs incomplets ou il etait organisateur et des joueurs n'ont pas paye)
4. Montrer le message d'erreur / blocage
5. Expliquer : en tant qu'organisateur, si le match n'est pas complet (moins de 4 joueurs payes), l'organisateur est responsable du manque a gagner

**Point cle :** Le systeme de dettes responsabilise les organisateurs. Georges doit regler ses 45 EUR avant de pouvoir re-organiser.

---

## Scenario 6 : Reservation d'un match prive

**Connexion :** alice@test.be

1. Aller sur la page de reservation
2. Choisir **Brussels Padel Center**, un terrain et un creneau disponible
3. Creer un match **prive**
4. Ajouter des participants : Bob, Claire, David
5. Voir le match dans "Mes matchs" avec le statut **Prive - 4/4**
6. Chaque participant doit payer 15 EUR
7. Simuler le paiement d'Alice (organisatrice)
8. Observer la progression : 1/4 paye, 2/4 paye, etc.

**Point cle :** Dans un match prive, seul l'organisateur peut inviter des joueurs.

---

## Scenario 7 : Match public - premier paye, premier servi

**Connexion :** claire@test.be

1. Creer un match **public** a Bruxelles dans quelques jours
2. Se deconnecter

**Connexion :** david@test.be

3. Voir le match public de Claire dans la liste des matchs disponibles
4. **Rejoindre** le match public
5. Payer les 15 EUR
6. Observer : David est confirme dans le match

**Connexion :** kevin@test.be

7. Rejoindre le meme match public
8. Payer les 15 EUR

**Connexion :** julie@test.be

9. Rejoindre le match → 4/4, le match passe en statut **Complet**

**Point cle :** Les matchs publics sont ouverts a tous. Premier paye = premier servi.

---

## Scenario 8 : Restriction membre site (Emma)

**Connexion :** emma@test.be

1. Emma est membre **Site** rattachee a Brussels Padel Center
2. Tenter de reserver a **Brussels** → OK
3. Tenter de reserver a **Liege** → **Refuse** (membre site restreint)
4. Montrer la fenetre de reservation : seul le site de Bruxelles est disponible

**Connexion :** alice@test.be (Global)

5. Alice est membre **Global** : elle peut reserver a Bruxelles ET Liege
6. Montrer les deux sites disponibles

**Point cle :** Les categories de membres determinent l'acces aux sites et la fenetre de reservation (Global=J-21, Site=J-14, Free=J-5).

---

## Scenario 9 : Fenetre de reservation par categorie

**Connexion :** alice@test.be (Global - G1001)

1. Voir les creneaux disponibles : jusqu'a **J+21**

**Connexion :** emma@test.be (Site - S10001)

2. Voir les creneaux disponibles : jusqu'a **J+14** seulement

**Connexion :** julie@test.be (Free - L10002)

3. Voir les creneaux disponibles : jusqu'a **J+5** seulement

**Point cle :** Les membres premium (Global) ont un avantage sur la reservation anticipee.

---

## Scenario 10 : Mecanisme J-1 et exclusion des impayes

> Ce scenario decrit le processus automatique, a montrer via les donnees seedees.

1. **Connexion :** admin@test.be
2. Aller dans **Vue operationnelle** > Brussels Padel Center
3. Observer les matchs de demain :
   - Match prive de David : 2/4 joueurs, tous impayes → **alerte J-1**
   - Match public de Claire : 3/4 joueurs, impayes → **alerte participants impayes**
4. Expliquer le mecanisme automatique :
   - A J-1, les matchs prives non complets passent en **public** (pour trouver des joueurs)
   - Les participants impayes sont **exclus** si leur paiement n'arrive pas
   - Si le match se joue avec moins de 4 joueurs payes, l'organisateur recoit une **dette** pour le manque a gagner

---

## Scenario 11 : Historique des matchs (mixte prive/public)

**Connexion :** alice@test.be

1. Aller dans **Mes matchs**
2. Observer l'historique :
   - Matchs completes (payes, joues) sur les derniers mois
   - Mix de matchs publics et prives
   - Matchs annules affiches avec un style different
3. Trier par date, filtrer par statut

**Point cle :** Chaque joueur a une vision claire de son historique de jeu.

---

## Parcours de demo complet (20 min)

Pour une demo structuree, suivre cet ordre :

1. **Admin overview** (3 min) → Scenario 1 + 2
2. **Analytics** (2 min) → Scenario 3
3. **Gestion sites** (2 min) → Scenario 4
4. **Reservation privee** (3 min) → Scenario 6
5. **Match public** (3 min) → Scenario 7
6. **Restrictions membres** (2 min) → Scenario 8 + 9
7. **Joueur bloque** (2 min) → Scenario 5
8. **Mecanisme J-1** (3 min) → Scenario 10
