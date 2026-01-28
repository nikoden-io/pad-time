# P4 - Security-model

---

# P4 - Security-model

## Objectif

Définir le modèle d’accès (OIDC/OAuth2), les claims attendues, les rôles/scopes, et les règles d’autorisation par endpoint.

---

## Authentification

- Fournisseur : IdentityServer (OpenID Connect)
- Clients : Angular (web), Flutter (bonus)
- Flow : Authorization Code + PKCE
- API : protégée par access token JWT (`Authorization: Bearer ...`)

---

## Scopes (OAuth2)

- `padel_api` : accès API standard (lecture + actions user)
- `padel_admin` : endpoints admin
- `padel_analytics` : endpoints analytics admin

Règle : un token peut porter plusieurs scopes.

---

## Rôles (claim `role`)

- `user`
- `admin_site`
- `admin_global`

---

## Claims minimales (obligatoires)

- `sub` : identifiant technique (OIDC subject)
- `role` : rôle applicatif
- `matricule` : identifiant métier (audit/UI)
- `member_category` : `global` | `site` | `free`
- `site_id` :
    - requis si `role=admin_site`
    - requis si `member_category=site`
    - absent sinon

---

## Stratégie d’autorisation

- RBAC : contrôle par rôle (`role`)
- ABAC : contrôle par attribut (`site_id`, `member_category`)

Règles non négociables :

- validation JWT côté API : signature + `iss` + `aud` + `exp`
- filtrage serveur obligatoire (jamais front)
- `admin_site` ne sort jamais du périmètre `site_id` du token

---

## Règles métier liées à la sécurité (rappels)

- `member_category=site` : création match uniquement sur `site_id` du token
- dette organisateur > 0 : création match interdite (403 `billing.organizer_debt_block`)
- fenêtres réservation :
    - global : J-21
    - site : J-14
    - free : J-5

---

## Matrice d’accès par endpoint (v1)

### Identity

- `GET /me`
    - scopes : `padel_api`
    - rôles : any authenticated

### Référentiel / Disponibilités

- `GET /sites`
    - scopes : `padel_api`
    - rôles : any authenticated
- `GET /sites/{siteId}/courts`
    - scopes : `padel_api`
    - rôles : any authenticated
    - ABAC option : si on restreint, alors member_category=site => siteId == claim
- `GET /availability`
    - scopes : `padel_api`
    - rôles : any authenticated
    - ABAC option similaire

### Booking

- `POST /matches`
    - scopes : `padel_api`
    - rôles : `user|admin_site|admin_global`
    - ABAC : member_category=site => siteId == claim site_id
    - règles métier : dette=0 + fenêtre réservation
- `GET /matches`
    - scopes : `padel_api`
    - rôles : any authenticated
    - contrainte : `scope=site` => admin uniquement
- `GET /matches/{matchId}`
    - scopes : `padel_api`
    - rôles : any authenticated
    - privé : participants + admins (site/global)
- `POST /matches/{matchId}/join`
    - scopes : `padel_api`
    - rôles : any authenticated
    - règle : match public + place dispo + idempotence
- `POST /matches/{matchId}/cancel`
    - scopes : `padel_api`
    - rôles :
        - `admin_global` : ok
        - `admin_site` : ok si site scope
        - `user` : ok si organisateur et non locked

### Paiements

- `GET /payments/{paymentId}`
    - scopes : `padel_api`
    - rôles : owner ou admin (site/global)

### Admin

- `GET /admin/sites/{siteId}/overview`
    - scopes : `padel_admin`
    - rôles : `admin_site|admin_global`
    - ABAC : admin_site => siteId == claim site_id

### Analytics

- `GET /admin/analytics/revenue`
    - scopes : `padel_analytics`
    - rôles : `admin_site|admin_global`
    - ABAC : admin_site => site filtré serveur (siteId forcé)

---

## Erreurs sécurité (ProblemDetails.type)

- `security.unauthenticated` (401)
- `security.forbidden` (403 générique)
- `booking.site_scope_violation` (403)