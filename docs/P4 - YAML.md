# P4 - YAML

```yaml
openapi: 3.0.3
info:
  title: Padel Booking API
  version: "1.0.0"
  description: >
    API REST v1 pour réservation de terrains de padel multi-sites avec OIDC (IdentityServer),
    paiements mock, dettes organisateur, jobs J-1 et analytics par agrégats.
servers:
  - url: /api/v1

tags:
  - name: Identity
  - name: Sites
  - name: Availability
  - name: Matches
  - name: Payments
  - name: Admin
  - name: Analytics

security:
  - bearerAuth: []

components:
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT

  headers:
    X-Correlation-Id:
      description: Identifiant de corrélation (propagé dans logs/traces).
      schema:
        type: string

  responses:
    Problem:
      description: Erreur standard (RFC7807 ProblemDetails)
      headers:
        X-Correlation-Id:
          $ref: "#/components/headers/X-Correlation-Id"
      content:
        application/problem+json:
          schema:
            $ref: "#/components/schemas/ProblemDetails"

  schemas:
    ProblemDetails:
      type: object
      additionalProperties: true
      properties:
        type:
          type: string
          description: Code stable (ex: booking.slot_conflict)
        title:
          type: string
        status:
          type: integer
          format: int32
        detail:
          type: string
        instance:
          type: string
        traceId:
          type: string
        errors:
          type: object
          additionalProperties:
            type: array
            items:
              type: string

    PageMeta:
      type: object
      required: [page, pageSize, total]
      properties:
        page:
          type: integer
          minimum: 1
        pageSize:
          type: integer
          minimum: 1
          maximum: 200
        total:
          type: integer
          minimum: 0

    PagedMatchesResponse:
      type: object
      required: [items, page, pageSize, total]
      properties:
        items:
          type: array
          items:
            $ref: "#/components/schemas/MatchDto"
        page:
          type: integer
          minimum: 1
        pageSize:
          type: integer
          minimum: 1
          maximum: 200
        total:
          type: integer
          minimum: 0

    MemberCategory:
      type: string
      enum: [global, site, free]

    Role:
      type: string
      enum: [user, admin_site, admin_global]

    MatchType:
      type: string
      enum: [private, public]

    MatchStatus:
      type: string
      enum: [draft, private, public, full, locked, completed, cancelled]

    PaymentStatus:
      type: string
      enum: [pending, paid, failed]

    ParticipantRole:
      type: string
      enum: [organizer, player]

    ParticipantPaymentStatus:
      type: string
      enum: [unpaid, pending, paid, failed, excluded]

    MeDto:
      type: object
      required: [subject, matricule, category, role]
      properties:
        subject:
          type: string
          description: OIDC sub
        matricule:
          type: string
        category:
          $ref: "#/components/schemas/MemberCategory"
        role:
          $ref: "#/components/schemas/Role"
        siteId:
          type: string
          nullable: true

    SiteSummaryDto:
      type: object
      required: [siteId, name, timezone]
      properties:
        siteId:
          type: string
        name:
          type: string
        timezone:
          type: string

    CourtDto:
      type: object
      required: [courtId, label, active]
      properties:
        courtId:
          type: string
        label:
          type: string
        active:
          type: boolean

    SlotDto:
      type: object
      required: [startAt, endAt, available]
      properties:
        startAt:
          type: string
          format: date-time
        endAt:
          type: string
          format: date-time
        available:
          type: boolean

    AvailabilityResponse:
      type: object
      required: [siteId, date, slots]
      properties:
        siteId:
          type: string
        date:
          type: string
          format: date
        slots:
          type: array
          items:
            $ref: "#/components/schemas/SlotDto"

    ParticipantDto:
      type: object
      required: [memberId, matricule, role, paymentStatus]
      properties:
        memberId:
          type: string
        matricule:
          type: string
        role:
          $ref: "#/components/schemas/ParticipantRole"
        paymentStatus:
          $ref: "#/components/schemas/ParticipantPaymentStatus"

    MatchDto:
      type: object
      required:
        - matchId
        - siteId
        - courtId
        - startAt
        - endAt
        - type
        - status
        - organizerMemberId
        - participants
        - priceTotalCents
      properties:
        matchId:
          type: string
        siteId:
          type: string
        courtId:
          type: string
        startAt:
          type: string
          format: date-time
        endAt:
          type: string
          format: date-time
        type:
          $ref: "#/components/schemas/MatchType"
        status:
          $ref: "#/components/schemas/MatchStatus"
        organizerMemberId:
          type: string
        participants:
          type: array
          items:
            $ref: "#/components/schemas/ParticipantDto"
        priceTotalCents:
          type: integer
          minimum: 0
          example: 6000

    CreateMatchRequest:
      type: object
      required: [siteId, courtId, startAt, type]
      properties:
        siteId:
          type: string
        courtId:
          type: string
        startAt:
          type: string
          format: date-time
        type:
          $ref: "#/components/schemas/MatchType"
        privateParticipantsMatricules:
          type: array
          description: Uniquement si type=private. 0..3 matricules ajoutés par l'organisateur.
          items:
            type: string
          maxItems: 3

    CreateMatchResponse:
      type: object
      required: [matchId]
      properties:
        matchId:
          type: string

    JoinPublicMatchRequest:
      type: object
      required: [idempotencyKey]
      properties:
        idempotencyKey:
          type: string
          description: Clé client d'idempotence (uuid recommandé)

    CreatePaymentResponse:
      type: object
      required: [paymentId, status]
      properties:
        paymentId:
          type: string
        status:
          $ref: "#/components/schemas/PaymentStatus"

    PaymentDto:
      type: object
      required: [paymentId, matchId, memberId, amountCents, status, createdAt]
      properties:
        paymentId:
          type: string
        matchId:
          type: string
        memberId:
          type: string
        amountCents:
          type: integer
          minimum: 1
        status:
          $ref: "#/components/schemas/PaymentStatus"
        createdAt:
          type: string
          format: date-time

    AdminAlertDto:
      type: object
      required: [code, message]
      properties:
        code:
          type: string
        message:
          type: string

    AdminSiteOverviewResponse:
      type: object
      required: [siteId, alerts]
      properties:
        siteId:
          type: string
        alerts:
          type: array
          items:
            $ref: "#/components/schemas/AdminAlertDto"

    DashboardRevenueDto:
      type: object
      required: [from, to, currency, items]
      properties:
        from:
          type: string
          format: date
        to:
          type: string
          format: date
        currency:
          type: string
          example: EUR
        items:
          type: array
          items:
            type: object
            required: [date, revenueCents]
            properties:
              date:
                type: string
                format: date
              revenueCents:
                type: integer
                minimum: 0

paths:
  /me:
    get:
      tags: [Identity]
      summary: Informations utilisateur courant (projection claims)
      responses:
        "200":
          description: OK
          headers:
            X-Correlation-Id:
              $ref: "#/components/headers/X-Correlation-Id"
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/MeDto"
        "401":
          $ref: "#/components/responses/Problem"

  /sites:
    get:
      tags: [Sites]
      summary: Liste des sites visibles
      responses:
        "200":
          description: OK
          headers:
            X-Correlation-Id:
              $ref: "#/components/headers/X-Correlation-Id"
          content:
            application/json:
              schema:
                type: array
                items:
                  $ref: "#/components/schemas/SiteSummaryDto"
        "401":
          $ref: "#/components/responses/Problem"

  /sites/{siteId}/courts:
    get:
      tags: [Sites]
      summary: Liste des terrains d'un site
      parameters:
        - in: path
          name: siteId
          required: true
          schema: { type: string }
      responses:
        "200":
          description: OK
          headers:
            X-Correlation-Id:
              $ref: "#/components/headers/X-Correlation-Id"
          content:
            application/json:
              schema:
                type: array
                items:
                  $ref: "#/components/schemas/CourtDto"
        "401":
          $ref: "#/components/responses/Problem"
        "403":
          $ref: "#/components/responses/Problem"
        "404":
          $ref: "#/components/responses/Problem"

  /availability:
    get:
      tags: [Availability]
      summary: Disponibilités calculées (slots) pour un site et une date
      parameters:
        - in: query
          name: siteId
          required: true
          schema: { type: string }
        - in: query
          name: date
          required: true
          schema: { type: string, format: date }
        - in: query
          name: courtId
          required: false
          schema: { type: string }
      responses:
        "200":
          description: OK
          headers:
            X-Correlation-Id:
              $ref: "#/components/headers/X-Correlation-Id"
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/AvailabilityResponse"
        "400":
          $ref: "#/components/responses/Problem"
        "401":
          $ref: "#/components/responses/Problem"
        "403":
          $ref: "#/components/responses/Problem"

  /matches:
    post:
      tags: [Matches]
      summary: Créer un match (privé/public)
      description: >
        Applique règles de réservation (J-21/J-14/J-5), dette organisateur, fermetures,
        cohérence site/terrain et anti double-booking.
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: "#/components/schemas/CreateMatchRequest"
      responses:
        "201":
          description: Created
          headers:
            X-Correlation-Id:
              $ref: "#/components/headers/X-Correlation-Id"
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/CreateMatchResponse"
        "400":
          $ref: "#/components/responses/Problem"
        "401":
          $ref: "#/components/responses/Problem"
        "403":
          $ref: "#/components/responses/Problem"
        "409":
          description: Conflit (slot déjà réservé)
          $ref: "#/components/responses/Problem"

    get:
      tags: [Matches]
      summary: Rechercher des matches
      parameters:
        - in: query
          name: scope
          required: false
          schema:
            type: string
            enum: [public, mine, site]
          description: public=publics, mine=participant, site=admin uniquement
        - in: query
          name: siteId
          required: false
          schema: { type: string }
        - in: query
          name: from
          required: false
          schema: { type: string, format: date-time }
        - in: query
          name: to
          required: false
          schema: { type: string, format: date-time }
        - in: query
          name: page
          required: false
          schema: { type: integer, minimum: 1, default: 1 }
        - in: query
          name: pageSize
          required: false
          schema: { type: integer, minimum: 1, maximum: 200, default: 50 }
      responses:
        "200":
          description: OK
          headers:
            X-Correlation-Id:
              $ref: "#/components/headers/X-Correlation-Id"
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/PagedMatchesResponse"
        "400":
          $ref: "#/components/responses/Problem"
        "401":
          $ref: "#/components/responses/Problem"
        "403":
          $ref: "#/components/responses/Problem"

  /matches/{matchId}:
    get:
      tags: [Matches]
      summary: Détail match
      parameters:
        - in: path
          name: matchId
          required: true
          schema: { type: string }
      responses:
        "200":
          description: OK
          headers:
            X-Correlation-Id:
              $ref: "#/components/headers/X-Correlation-Id"
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/MatchDto"
        "401":
          $ref: "#/components/responses/Problem"
        "403":
          $ref: "#/components/responses/Problem"
        "404":
          $ref: "#/components/responses/Problem"

  /matches/{matchId}/join:
    post:
      tags: [Matches]
      summary: Rejoindre un match public (paiement mock)
      description: >
        Réservation de place atomique et idempotence via idempotencyKey.
        Conflit si match complet ou pas public.
      parameters:
        - in: path
          name: matchId
          required: true
          schema: { type: string }
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: "#/components/schemas/JoinPublicMatchRequest"
      responses:
        "200":
          description: OK
          headers:
            X-Correlation-Id:
              $ref: "#/components/headers/X-Correlation-Id"
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/CreatePaymentResponse"
        "400":
          $ref: "#/components/responses/Problem"
        "401":
          $ref: "#/components/responses/Problem"
        "403":
          $ref: "#/components/responses/Problem"
        "409":
          $ref: "#/components/responses/Problem"

  /matches/{matchId}/cancel:
    post:
      tags: [Matches]
      summary: Annuler un match (MVP)
      description: >
        MVP: admin_global toujours; admin_site si match du site; organisateur si non locked.
      parameters:
        - in: path
          name: matchId
          required: true
          schema: { type: string }
      responses:
        "204":
          description: No Content
          headers:
            X-Correlation-Id:
              $ref: "#/components/headers/X-Correlation-Id"
        "401":
          $ref: "#/components/responses/Problem"
        "403":
          $ref: "#/components/responses/Problem"
        "404":
          $ref: "#/components/responses/Problem"
        "409":
          $ref: "#/components/responses/Problem"

  /payments/{paymentId}:
    get:
      tags: [Payments]
      summary: Détail paiement
      parameters:
        - in: path
          name: paymentId
          required: true
          schema: { type: string }
      responses:
        "200":
          description: OK
          headers:
            X-Correlation-Id:
              $ref: "#/components/headers/X-Correlation-Id"
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/PaymentDto"
        "401":
          $ref: "#/components/responses/Problem"
        "403":
          $ref: "#/components/responses/Problem"
        "404":
          $ref: "#/components/responses/Problem"

  /admin/sites/{siteId}/overview:
    get:
      tags: [Admin]
      summary: Vue admin site (alertes opérationnelles)
      parameters:
        - in: path
          name: siteId
          required: true
          schema: { type: string }
      responses:
        "200":
          description: OK
          headers:
            X-Correlation-Id:
              $ref: "#/components/headers/X-Correlation-Id"
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/AdminSiteOverviewResponse"
        "401":
          $ref: "#/components/responses/Problem"
        "403":
          $ref: "#/components/responses/Problem"
        "404":
          $ref: "#/components/responses/Problem"

  /admin/analytics/revenue:
    get:
      tags: [Analytics]
      summary: CA agrégé (admin)
      parameters:
        - in: query
          name: siteId
          required: false
          schema: { type: string }
        - in: query
          name: from
          required: true
          schema: { type: string, format: date }
        - in: query
          name: to
          required: true
          schema: { type: string, format: date }
      responses:
        "200":
          description: OK
          headers:
            X-Correlation-Id:
              $ref: "#/components/headers/X-Correlation-Id"
          content:
            application/json:
              schema:
                $ref: "#/components/schemas/DashboardRevenueDto"
        "400":
          $ref: "#/components/responses/Problem"
        "401":
          $ref: "#/components/responses/Problem"
        "403":
          $ref: "#/components/responses/Problem"

```