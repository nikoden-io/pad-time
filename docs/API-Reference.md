# PadTime API Reference

Complete REST API documentation for the PadTime padel court booking platform.

**Version:** v1
**Base URL:** `https://api.padtime.io/api/v1`

---

## Table of Contents

- [Overview](#overview)
- [Authentication](#authentication)
- [Authorization Policies](#authorization-policies)
- [Pagination](#pagination)
- [Endpoints](#endpoints)
  - [Matches](#matches)
  - [Availability](#availability)
  - [Payments](#payments)
  - [Sites](#sites)
  - [Admin](#admin)
  - [Me](#me)
- [Error Responses](#error-responses)
- [Enums](#enums)

---

## Overview

The PadTime API is a RESTful JSON API that powers a padel court booking platform. It supports match creation and joining, court availability queries, payment processing, site management, and administrative operations.

| Property        | Value                          |
|-----------------|--------------------------------|
| Base URL        | `https://api.padtime.io/api/v1`|
| Content-Type    | `application/json`             |
| Error Format    | RFC 7807 Problem Details       |
| Authentication  | JWT Bearer Token               |
| Pagination      | Page-based (`page`, `pageSize`)|

All timestamps are in **UTC** and follow ISO 8601 format. All monetary amounts are expressed in **cents** (e.g., `1200` = 12.00 EUR).

---

## Authentication

PadTime uses JWT Bearer tokens for authentication. Tokens are obtained through an external identity provider and must be included in the `Authorization` header of every authenticated request.

### Required Headers

```http
Authorization: Bearer <jwt_token>
Content-Type: application/json
```

### JWT Claims

The following claims are extracted from the JWT and used for authorization:

| Claim       | Description                                       |
|-------------|---------------------------------------------------|
| `sub`       | Unique subject identifier from the identity provider |
| `matricule` | Member matricule (e.g., `G0001`, `S00042`, `L00007`) |
| `category`  | Member category: `global`, `site`, or `free`      |
| `role`      | User role: `user`, `site_admin`, or `global_admin` |
| `site_id`   | Assigned site ID (for site members and site admins) |

---

## Authorization Policies

Endpoints are protected by one of the following authorization policies:

| Policy                  | Description                                                     |
|-------------------------|-----------------------------------------------------------------|
| `RequireUser`           | Any authenticated user.                                         |
| `RequireAdmin`          | Site admin or global admin.                                     |
| `RequireGlobalAdmin`    | Global admin only.                                              |
| `RequireSiteAdmin`      | Site admin (or global admin).                                   |
| `RequireSiteAccess`     | User must have access to the requested site.                    |
| `RequireSiteManagement` | Management-level permissions for the requested site.            |

---

## Pagination

Paginated endpoints return a `PagedResult<T>` envelope:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 47,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

| Parameter  | Type   | Default | Description                 |
|------------|--------|---------|-----------------------------|
| `page`     | int    | 1       | Page number (starts at 1).  |
| `pageSize` | int    | 20      | Number of items per page.   |

---

## Endpoints

### Matches

Base path: `/api/v1/matches`
Default authorization: `RequireUser`

---

#### List Matches

Retrieves matches based on a specified scope.

```
GET /api/v1/matches
```

**Authorization:** RequireUser

**Query Parameters:**

| Parameter  | Type     | Default    | Description                                              |
|------------|----------|------------|----------------------------------------------------------|
| `scope`    | string   | `public`   | One of `public`, `mine`, or `site`.                      |
| `siteId`   | guid     | _optional_ | Required when `scope=site`. Optional filter for `public`.|
| `from`     | datetime | _optional_ | Start of search window (UTC).                            |
| `to`       | datetime | _optional_ | End of search window (UTC).                              |
| `page`     | int      | 1          | Page number.                                             |
| `pageSize` | int      | 20         | Results per page.                                        |

**Scope Behavior:**

- `public` -- Public matches available to join. Defaults to now through now + 30 days.
- `mine` -- Matches where the current user is a participant (organizer or joined).
- `site` -- All matches for a site. Requires admin role and a `siteId` parameter.

**Response (scope=public):** `200 OK`

```json
[
  {
    "matchId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "courtId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "startAtUtc": "2026-04-05T14:00:00Z",
    "endAtUtc": "2026-04-05T15:30:00Z",
    "status": "Public",
    "organizerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "priceTotalCents": 4800,
    "participantCount": 2,
    "availableSeats": 2,
    "participants": [
      {
        "memberId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "matricule": "G0001",
        "role": "Organizer",
        "paymentStatus": "Paid"
      },
      {
        "memberId": "b2d4f6a8-1234-5678-9abc-def012345678",
        "matricule": "L00007",
        "role": "Participant",
        "paymentStatus": "Paid"
      }
    ]
  }
]
```

**Response (scope=mine):** `200 OK`

```json
[
  {
    "matchId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "courtId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "startAtUtc": "2026-04-05T14:00:00Z",
    "endAtUtc": "2026-04-05T15:30:00Z",
    "type": "Public",
    "status": "Full",
    "organizerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "priceTotalCents": 4800,
    "participants": [
      {
        "memberId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "matricule": "G0001",
        "role": "Organizer",
        "paymentStatus": "Paid"
      }
    ]
  }
]
```

**Response (scope=site):** `200 OK`

```json
[
  {
    "matchId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "courtId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "startAtUtc": "2026-04-05T14:00:00Z",
    "endAtUtc": "2026-04-05T15:30:00Z",
    "type": "Private",
    "status": "Private",
    "organizerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "priceTotalCents": 4800,
    "participantCount": 3
  }
]
```

**Status Codes:**

| Code | Description                                   |
|------|-----------------------------------------------|
| 200  | Matches successfully retrieved.               |
| 400  | Invalid scope or missing required parameter.  |
| 401  | User is not authenticated.                    |
| 403  | Scope requires admin role.                    |

---

#### Create Match

Creates a new match. The authenticated user becomes the organizer. Supports both public matches (anyone can join by paying) and private matches (organizer invites participants by matricule).

```
POST /api/v1/matches
```

**Authorization:** RequireUser

**Request Body:**

```json
{
  "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "courtId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "startAt": "2026-04-05T14:00:00Z",
  "type": "private",
  "privateParticipantsMatricules": ["S00042", "G0012", "L00007"]
}
```

| Field                          | Type     | Required | Description                                             |
|--------------------------------|----------|----------|---------------------------------------------------------|
| `siteId`                       | guid     | Yes      | Site where the match takes place.                       |
| `courtId`                      | guid     | Yes      | Court to book.                                          |
| `startAt`                      | datetime | Yes      | Match start time (UTC).                                 |
| `type`                         | string   | Yes      | `"public"` or `"private"`.                              |
| `privateParticipantsMatricules`| string[] | No       | Matricules of initial participants (private matches only).|

**Response:** `201 Created`

```json
{
  "matchId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

**Headers:**

```
Location: /api/v1/matches/a1b2c3d4-e5f6-7890-abcd-ef1234567890
```

**Status Codes:**

| Code | Description                                                    |
|------|----------------------------------------------------------------|
| 201  | Match successfully created.                                    |
| 400  | Invalid request or validation failure.                         |
| 401  | User is not authenticated.                                     |
| 403  | User is not authorized (reservation window denied, site scope violation, debt block, inactive account). |
| 409  | Slot conflict or business rule violation (slot already booked).|

---

#### Get Match

Retrieves the details of a specific match. Private matches are only visible to participants and administrators.

```
GET /api/v1/matches/{matchId}
```

**Authorization:** RequireUser

**Path Parameters:**

| Parameter | Type | Description                 |
|-----------|------|-----------------------------|
| `matchId` | guid | Identifier of the match.    |

**Response:** `200 OK`

```json
{
  "matchId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "courtId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "startAtUtc": "2026-04-05T14:00:00Z",
  "endAtUtc": "2026-04-05T15:30:00Z",
  "type": "Private",
  "status": "Private",
  "organizerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "priceTotalCents": 4800,
  "participants": [
    {
      "memberId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "matricule": "G0001",
      "role": "Organizer",
      "paymentStatus": "Paid"
    },
    {
      "memberId": "b2d4f6a8-1234-5678-9abc-def012345678",
      "matricule": "S00042",
      "role": "Participant",
      "paymentStatus": "Unpaid"
    },
    {
      "memberId": "c3e5a7b9-2345-6789-abcd-ef0123456789",
      "matricule": "G0012",
      "role": "Participant",
      "paymentStatus": "Pending"
    },
    {
      "memberId": "d4f6b8c0-3456-7890-bcde-f01234567890",
      "matricule": "L00007",
      "role": "Participant",
      "paymentStatus": "Paid"
    }
  ]
}
```

**Status Codes:**

| Code | Description                              |
|------|------------------------------------------|
| 200  | Match successfully retrieved.            |
| 401  | User is not authenticated.               |
| 403  | User is not authorized to view this match.|
| 404  | Match was not found.                     |

---

#### Get User Matches

Retrieves matches where the current authenticated user is a participant (organized or joined).

```
GET /api/v1/matches/user
```

**Authorization:** RequireUser

**Query Parameters:**

| Parameter  | Type     | Default    | Description                                     |
|------------|----------|------------|-------------------------------------------------|
| `fromUtc`  | datetime | _optional_ | Only return matches starting on or after this date.|
| `page`     | int      | 1          | Page number.                                    |
| `pageSize` | int      | 20         | Results per page.                               |

**Response:** `200 OK`

```json
[
  {
    "matchId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "courtId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "startAtUtc": "2026-04-05T14:00:00Z",
    "endAtUtc": "2026-04-05T15:30:00Z",
    "type": "Public",
    "status": "Full",
    "organizerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "priceTotalCents": 4800,
    "participants": [
      {
        "memberId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "matricule": "G0001",
        "role": "Organizer",
        "paymentStatus": "Paid"
      }
    ]
  }
]
```

**Status Codes:**

| Code | Description                   |
|------|-------------------------------|
| 200  | Matches successfully retrieved.|
| 401  | User is not authenticated.    |

---

#### Get Public Matches

Retrieves paginated public matches available for joining. Returns matches with status `Public` or `Full`, ordered by start time. Defaults to a 30-day window from now if no date range is provided.

```
GET /api/v1/matches/public
```

**Authorization:** RequireUser

**Query Parameters:**

| Parameter  | Type     | Default         | Description                           |
|------------|----------|-----------------|---------------------------------------|
| `siteId`   | guid     | _optional_      | Filter by site.                       |
| `fromUtc`  | datetime | now             | Start of search window (UTC).         |
| `toUtc`    | datetime | now + 30 days   | End of search window (UTC).           |
| `page`     | int      | 1               | Page number.                          |
| `pageSize` | int      | 20              | Results per page.                     |

**Response:** `200 OK`

```json
[
  {
    "matchId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "courtId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "startAtUtc": "2026-04-12T10:00:00Z",
    "endAtUtc": "2026-04-12T11:30:00Z",
    "status": "Public",
    "organizerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "priceTotalCents": 4800,
    "participantCount": 1,
    "availableSeats": 3,
    "participants": [
      {
        "memberId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "matricule": "G0001",
        "role": "Organizer",
        "paymentStatus": "Paid"
      }
    ]
  }
]
```

**Status Codes:**

| Code | Description                   |
|------|-------------------------------|
| 200  | Matches successfully retrieved.|
| 401  | User is not authenticated.    |

---

#### Add Participant

Adds a participant to a **private** match by their matricule. Only the match organizer can perform this action.

```
POST /api/v1/matches/{matchId}/participants
```

**Authorization:** RequireUser (must be the match organizer)

**Path Parameters:**

| Parameter | Type | Description              |
|-----------|------|--------------------------|
| `matchId` | guid | Identifier of the match. |

**Request Body:**

```json
{
  "matricule": "S00042"
}
```

| Field       | Type   | Required | Description                       |
|-------------|--------|----------|-----------------------------------|
| `matricule` | string | Yes      | Matricule of the member to add.   |

**Response:** `204 No Content`

**Status Codes:**

| Code | Description                                    |
|------|------------------------------------------------|
| 204  | Participant successfully added.                |
| 400  | Invalid matricule format.                      |
| 401  | User is not authenticated.                     |
| 403  | Not the organizer of this match.               |
| 404  | Match or member not found.                     |
| 409  | Match is full or participant already registered.|

---

#### Join Match

Joins a **public** match as a participant. Immediate payment is required. The operation is idempotent using the provided idempotency key -- if retried with the same key, the original result is returned.

```
POST /api/v1/matches/{matchId}/join
```

**Authorization:** RequireUser

**Path Parameters:**

| Parameter | Type | Description              |
|-----------|------|--------------------------|
| `matchId` | guid | Identifier of the match. |

**Request Body:**

```json
{
  "idempotencyKey": "join-G0001-a1b2c3d4-20260405"
}
```

| Field            | Type   | Required | Description                                  |
|------------------|--------|----------|----------------------------------------------|
| `idempotencyKey` | string | Yes      | Client-generated key for idempotent retries.  |

**Response:** `200 OK`

```json
{
  "paymentId": "e8f9a0b1-c2d3-4e5f-6789-0abcdef12345",
  "status": "Paid"
}
```

**Status Codes:**

| Code | Description                                                        |
|------|--------------------------------------------------------------------|
| 200  | Successfully joined the match.                                     |
| 400  | Invalid request.                                                   |
| 401  | User is not authenticated.                                         |
| 403  | Match is not public, match is locked, reservation window denied, or inactive account. |
| 404  | Match was not found.                                               |
| 409  | Match is full, already a participant, or idempotency conflict.     |

---

#### Cancel Match

Cancels an existing match. Only the organizer can cancel before the match is locked. Administrators may cancel matches according to their scope.

```
POST /api/v1/matches/{matchId}/cancel
```

**Authorization:** RequireUser (must be organizer or admin)

**Path Parameters:**

| Parameter | Type | Description              |
|-----------|------|--------------------------|
| `matchId` | guid | Identifier of the match. |

**Request Body:** _None_

**Response:** `204 No Content`

**Status Codes:**

| Code | Description                                           |
|------|-------------------------------------------------------|
| 204  | Match successfully cancelled.                         |
| 401  | User is not authenticated.                            |
| 403  | User is not authorized to cancel this match.          |
| 404  | Match was not found.                                  |
| 409  | Match cannot be cancelled (invalid state transition). |

---

### Availability

Base path: `/api/v1/availability`
Default authorization: `RequireUser`

---

#### Get Availability

Returns available time slots for a site on a specific date, optionally filtered by court.

```
GET /api/v1/availability
```

**Authorization:** RequireUser

**Query Parameters:**

| Parameter | Type     | Required | Description                          |
|-----------|----------|----------|--------------------------------------|
| `siteId`  | guid     | Yes      | Site to check availability for.      |
| `date`    | date     | Yes      | Date to check (format: `YYYY-MM-DD`).|
| `courtId` | guid     | No       | Filter to a specific court.          |

**Response:** `200 OK`

```json
{
  "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "date": "2026-04-05",
  "slots": [
    {
      "courtId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "courtLabel": "Court Central",
      "startAt": "2026-04-05T08:00:00Z",
      "endAt": "2026-04-05T09:30:00Z",
      "available": true
    },
    {
      "courtId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "courtLabel": "Court Central",
      "startAt": "2026-04-05T09:30:00Z",
      "endAt": "2026-04-05T11:00:00Z",
      "available": false
    },
    {
      "courtId": "8d0f7780-8536-51ef-a55c-f18gd2g01bf8",
      "courtLabel": "Court Panoramique",
      "startAt": "2026-04-05T08:00:00Z",
      "endAt": "2026-04-05T09:30:00Z",
      "available": true
    }
  ]
}
```

**Status Codes:**

| Code | Description                      |
|------|----------------------------------|
| 200  | Availability successfully returned.|
| 401  | User is not authenticated.       |

---

### Payments

Base path: `/api/v1/payments`
Default authorization: `RequireUser`

---

#### Get Payment

Retrieves a payment by its identifier. Only the payment owner or an admin can access the payment.

```
GET /api/v1/payments/{paymentId}
```

**Authorization:** RequireUser (owner or admin)

**Path Parameters:**

| Parameter   | Type | Description                 |
|-------------|------|-----------------------------|
| `paymentId` | guid | Identifier of the payment.  |

**Response:** `200 OK`

```json
{
  "paymentId": "e8f9a0b1-c2d3-4e5f-6789-0abcdef12345",
  "matchId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "memberId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "amountCents": 1200,
  "status": "Paid",
  "createdAtUtc": "2026-04-01T10:30:00Z"
}
```

**Status Codes:**

| Code | Description                           |
|------|---------------------------------------|
| 200  | Payment successfully retrieved.       |
| 401  | User is not authenticated.            |
| 404  | Payment not found or access denied.   |

---

#### Pay Match Participation

Pays the participation fee for a **private** match. Only the participant themselves can pay their own slot. The operation is idempotent via the provided idempotency key.

```
POST /api/v1/payments/matches/{matchId}/pay
```

**Authorization:** RequireUser (must be a participant)

**Path Parameters:**

| Parameter | Type | Description              |
|-----------|------|--------------------------|
| `matchId` | guid | Identifier of the match. |

**Request Body:**

```json
{
  "idempotencyKey": "pay-S00042-a1b2c3d4-20260401"
}
```

| Field            | Type   | Required | Description                                  |
|------------------|--------|----------|----------------------------------------------|
| `idempotencyKey` | string | Yes      | Client-generated key for idempotent retries.  |

**Response:** `200 OK`

```json
{
  "paymentId": "e8f9a0b1-c2d3-4e5f-6789-0abcdef12345",
  "status": "Paid"
}
```

**Status Codes:**

| Code | Description                                  |
|------|----------------------------------------------|
| 200  | Payment successful.                          |
| 401  | User is not authenticated.                   |
| 403  | Not a participant or payment already processed.|
| 404  | Match not found.                             |
| 409  | Idempotency conflict.                        |

---

### Sites

Base path: `/api/v1/sites`
Default authorization: `RequireUser`

---

#### Create Site

Creates a new padel site.

```
POST /api/v1/sites
```

**Authorization:** RequireGlobalAdmin

**Request Body:**

```json
{
  "name": "PadTime Brussels South",
  "streetNumber": "42",
  "street": "Avenue de la Padel",
  "postcode": "1060",
  "city": "Brussels",
  "country": "Belgium",
  "timezone": "Europe/Brussels"
}
```

| Field          | Type   | Required | Description                            |
|----------------|--------|----------|----------------------------------------|
| `name`         | string | Yes      | Display name of the site.              |
| `streetNumber` | string | Yes      | Street number.                         |
| `street`       | string | Yes      | Street name.                           |
| `postcode`     | string | Yes      | Postal code.                           |
| `city`         | string | Yes      | City.                                  |
| `country`      | string | Yes      | Country.                               |
| `timezone`     | string | Yes      | IANA timezone (e.g., `Europe/Brussels`).|

**Response:** `201 Created`

```json
{
  "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479"
}
```

**Headers:**

```
Location: /api/v1/sites/f47ac10b-58cc-4372-a567-0e02b2c3d479
```

**Status Codes:**

| Code | Description                     |
|------|---------------------------------|
| 201  | Site successfully created.      |
| 400  | Invalid request or validation failure. |
| 401  | User is not authenticated.      |
| 403  | User is not a global admin.     |

---

#### List Sites

Returns a paginated, filterable list of sites.

```
GET /api/v1/sites
```

**Authorization:** RequireUser

**Query Parameters:**

| Parameter    | Type   | Default    | Description                     |
|--------------|--------|------------|---------------------------------|
| `page`       | int    | 1          | Page number.                    |
| `pageSize`   | int    | 20         | Results per page.               |
| `searchTerm` | string | _optional_ | Search by name or address.      |
| `isActive`   | bool   | _optional_ | Filter by active status.        |
| `city`       | string | _optional_ | Filter by city.                 |
| `country`    | string | _optional_ | Filter by country.              |

**Response:** `200 OK`

```json
{
  "items": [
    {
      "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
      "name": "PadTime Brussels South",
      "streetNumber": "42",
      "street": "Avenue de la Padel",
      "postcode": "1060",
      "city": "Brussels",
      "country": "Belgium",
      "timezone": "Europe/Brussels",
      "isActive": true,
      "createdAtUtc": "2025-06-15T09:00:00Z",
      "courtCount": 4,
      "courts": [
        {
          "courtId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
          "label": "Court Central",
          "isActive": true
        },
        {
          "courtId": "8d0f7780-8536-51ef-a55c-f18gd2g01bf8",
          "label": "Court Panoramique",
          "isActive": true
        }
      ]
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 3,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

**Status Codes:**

| Code | Description                    |
|------|--------------------------------|
| 200  | Sites successfully retrieved.  |
| 400  | Invalid query parameters.      |
| 401  | User is not authenticated.     |

---

#### Get Site by ID

Returns detailed information about a specific site, including its courts, schedules, and closures.

```
GET /api/v1/sites/{siteId}
```

**Authorization:** RequireSiteAccess

**Path Parameters:**

| Parameter | Type | Description             |
|-----------|------|-------------------------|
| `siteId`  | guid | Identifier of the site. |

**Response:** `200 OK`

```json
{
  "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "name": "PadTime Brussels South",
  "streetNumber": "42",
  "street": "Avenue de la Padel",
  "postcode": "1060",
  "city": "Brussels",
  "country": "Belgium",
  "timezone": "Europe/Brussels",
  "isActive": true,
  "createdAtUtc": "2025-06-15T09:00:00Z",
  "updatedAtUtc": "2025-12-01T14:30:00Z",
  "courts": [
    {
      "courtId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "label": "Court Central",
      "isActive": true,
      "createdAtUtc": "2025-06-15T09:10:00Z"
    }
  ],
  "schedules": [
    {
      "scheduleId": "d1e2f3a4-b5c6-7890-1234-567890abcdef",
      "name": "Summer Hours",
      "validFrom": "2026-04-01",
      "validUntil": "2026-09-30",
      "openingTime": "07:00",
      "closingTime": "22:00",
      "applicableDays": [1, 2, 3, 4, 5, 6, 0],
      "priority": 10,
      "isActive": true,
      "createdAtUtc": "2025-12-01T14:30:00Z",
      "updatedAtUtc": null
    }
  ],
  "closures": [
    {
      "closureId": "a9b8c7d6-e5f4-3210-fedc-ba0987654321",
      "type": "FullDay",
      "reason": "PublicHoliday",
      "description": "Belgian National Day",
      "startDate": "2026-07-21",
      "endDate": "2026-07-21",
      "modifiedOpeningTime": null,
      "modifiedClosingTime": null,
      "affectedCourtIds": null,
      "createdAtUtc": "2026-01-15T08:00:00Z",
      "updatedAtUtc": null
    }
  ]
}
```

**Status Codes:**

| Code | Description                  |
|------|------------------------------|
| 200  | Site successfully retrieved. |
| 401  | User is not authenticated.   |
| 403  | User does not have access to this site. |
| 404  | Site not found.              |

---

#### Update Site

Updates an existing site's information.

```
PUT /api/v1/sites/{siteId}
```

**Authorization:** RequireSiteManagement

**Path Parameters:**

| Parameter | Type | Description             |
|-----------|------|-------------------------|
| `siteId`  | guid | Identifier of the site. |

**Request Body:**

```json
{
  "name": "PadTime Brussels South - Renovated",
  "streetNumber": "42",
  "street": "Avenue de la Padel",
  "postcode": "1060",
  "city": "Brussels",
  "country": "Belgium",
  "timezone": "Europe/Brussels"
}
```

| Field          | Type   | Required | Description                            |
|----------------|--------|----------|----------------------------------------|
| `name`         | string | Yes      | Display name of the site.              |
| `streetNumber` | string | Yes      | Street number.                         |
| `street`       | string | Yes      | Street name.                           |
| `postcode`     | string | Yes      | Postal code.                           |
| `city`         | string | Yes      | City.                                  |
| `country`      | string | Yes      | Country.                               |
| `timezone`     | string | Yes      | IANA timezone.                         |

**Response:** `204 No Content`

**Status Codes:**

| Code | Description                     |
|------|---------------------------------|
| 204  | Site successfully updated.      |
| 400  | Invalid request or validation failure. |
| 401  | User is not authenticated.      |
| 403  | User does not have management permissions. |
| 404  | Site not found.                 |

---

#### Delete Site

Deletes a site with safety checks. Fails if the site has active or future bookings.

```
DELETE /api/v1/sites/{siteId}
```

**Authorization:** RequireGlobalAdmin

**Path Parameters:**

| Parameter | Type | Description             |
|-----------|------|-------------------------|
| `siteId`  | guid | Identifier of the site. |

**Response:** `204 No Content`

**Status Codes:**

| Code | Description                                         |
|------|-----------------------------------------------------|
| 204  | Site successfully deleted.                          |
| 401  | User is not authenticated.                          |
| 403  | User is not a global admin.                         |
| 404  | Site not found.                                     |
| 409  | Cannot delete site with active or future bookings.  |

---

#### Deactivate Site

Deactivates a site. Use this when deletion is not possible due to existing bookings.

```
POST /api/v1/sites/{siteId}/deactivate
```

**Authorization:** RequireSiteManagement

**Path Parameters:**

| Parameter | Type | Description             |
|-----------|------|-------------------------|
| `siteId`  | guid | Identifier of the site. |

**Response:** `204 No Content`

**Status Codes:**

| Code | Description                     |
|------|---------------------------------|
| 204  | Site successfully deactivated.  |
| 400  | Invalid request.                |
| 401  | User is not authenticated.      |
| 403  | User does not have management permissions. |
| 404  | Site not found.                 |
| 409  | Site is already deactivated.    |

---

#### Activate Site

Activates a previously deactivated site.

```
POST /api/v1/sites/{siteId}/activate
```

**Authorization:** RequireSiteManagement

**Path Parameters:**

| Parameter | Type | Description             |
|-----------|------|-------------------------|
| `siteId`  | guid | Identifier of the site. |

**Response:** `204 No Content`

**Status Codes:**

| Code | Description                     |
|------|---------------------------------|
| 204  | Site successfully activated.    |
| 400  | Invalid request.                |
| 401  | User is not authenticated.      |
| 403  | User does not have management permissions. |
| 404  | Site not found.                 |
| 409  | Site is already active.         |

---

#### Get Site Statistics

Returns dashboard statistics for a site, including court utilization and booking trends.

```
GET /api/v1/sites/{siteId}/statistics
```

**Authorization:** RequireSiteAccess

**Path Parameters:**

| Parameter | Type | Description             |
|-----------|------|-------------------------|
| `siteId`  | guid | Identifier of the site. |

**Response:** `200 OK`

```json
{
  "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "siteName": "PadTime Brussels South",
  "totalCourts": 4,
  "activeCourts": 4,
  "totalBookingsThisMonth": 128,
  "totalBookingsLastMonth": 102,
  "bookingGrowthPercentage": 25.49,
  "upcomingBookingsToday": 6,
  "upcomingBookingsThisWeek": 34,
  "lastBookingDate": "2026-03-31T18:00:00Z",
  "courtUtilization": [
    {
      "courtId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "courtLabel": "Court Central",
      "bookingsThisMonth": 42,
      "utilizationPercentage": 68.5
    },
    {
      "courtId": "8d0f7780-8536-51ef-a55c-f18gd2g01bf8",
      "courtLabel": "Court Panoramique",
      "bookingsThisMonth": 38,
      "utilizationPercentage": 61.2
    }
  ],
  "recentBookingStats": [
    {
      "date": "2026-03-30",
      "bookingCount": 8,
      "uniqueUsers": 22
    },
    {
      "date": "2026-03-31",
      "bookingCount": 6,
      "uniqueUsers": 18
    }
  ]
}
```

**Status Codes:**

| Code | Description                    |
|------|--------------------------------|
| 200  | Statistics successfully returned.|
| 401  | User is not authenticated.     |
| 403  | User does not have access to this site. |
| 404  | Site not found.                |

---

#### List Courts

Returns all courts for a specific site.

```
GET /api/v1/sites/{siteId}/courts
```

**Authorization:** RequireSiteAccess

**Path Parameters:**

| Parameter | Type | Description             |
|-----------|------|-------------------------|
| `siteId`  | guid | Identifier of the site. |

**Response:** `200 OK`

```json
[
  {
    "courtId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "label": "Court Central",
    "isActive": true
  },
  {
    "courtId": "8d0f7780-8536-51ef-a55c-f18gd2g01bf8",
    "label": "Court Panoramique",
    "isActive": true
  },
  {
    "courtId": "9e1a8891-9647-62fg-b66d-g29he3h12cg9",
    "label": "Court 3",
    "isActive": false
  }
]
```

**Status Codes:**

| Code | Description                    |
|------|--------------------------------|
| 200  | Courts successfully retrieved. |
| 401  | User is not authenticated.     |
| 403  | User does not have access to this site. |
| 404  | Site not found.                |

---

#### Get Court by ID

Returns detailed information about a specific court.

```
GET /api/v1/sites/{siteId}/courts/{courtId}
```

**Authorization:** RequireSiteAccess

**Path Parameters:**

| Parameter | Type | Description              |
|-----------|------|--------------------------|
| `siteId`  | guid | Identifier of the site.  |
| `courtId` | guid | Identifier of the court. |

**Response:** `200 OK`

```json
{
  "courtId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "label": "Court Central",
  "isActive": true,
  "createdAtUtc": "2025-06-15T09:10:00Z"
}
```

**Status Codes:**

| Code | Description                    |
|------|--------------------------------|
| 200  | Court successfully retrieved.  |
| 401  | User is not authenticated.     |
| 403  | User does not have access to this site. |
| 404  | Court not found.               |

---

#### Create Court

Creates a new court for a site.

```
POST /api/v1/sites/{siteId}/courts
```

**Authorization:** RequireSiteManagement

**Path Parameters:**

| Parameter | Type | Description             |
|-----------|------|-------------------------|
| `siteId`  | guid | Identifier of the site. |

**Request Body:**

```json
{
  "label": "Court Panoramique"
}
```

| Field   | Type   | Required | Description            |
|---------|--------|----------|------------------------|
| `label` | string | Yes      | Display label for the court. |

**Response:** `201 Created`

```json
{
  "courtId": "8d0f7780-8536-51ef-a55c-f18gd2g01bf8"
}
```

**Headers:**

```
Location: /api/v1/sites/f47ac10b-58cc-4372-a567-0e02b2c3d479/courts/8d0f7780-8536-51ef-a55c-f18gd2g01bf8
```

**Status Codes:**

| Code | Description                     |
|------|---------------------------------|
| 201  | Court successfully created.     |
| 400  | Invalid request or validation failure. |
| 401  | User is not authenticated.      |
| 403  | User does not have management permissions. |
| 404  | Site not found.                 |
| 409  | A court with this label already exists for this site. |

---

#### Update Court

Updates an existing court's label.

```
PUT /api/v1/sites/{siteId}/courts/{courtId}
```

**Authorization:** RequireSiteManagement

**Path Parameters:**

| Parameter | Type | Description              |
|-----------|------|--------------------------|
| `siteId`  | guid | Identifier of the site.  |
| `courtId` | guid | Identifier of the court. |

**Request Body:**

```json
{
  "label": "Court Central - Indoor"
}
```

| Field   | Type   | Required | Description            |
|---------|--------|----------|------------------------|
| `label` | string | Yes      | New display label.     |

**Response:** `204 No Content`

**Status Codes:**

| Code | Description                     |
|------|---------------------------------|
| 204  | Court successfully updated.     |
| 400  | Invalid request or validation failure. |
| 401  | User is not authenticated.      |
| 403  | User does not have management permissions. |
| 404  | Court not found.                |
| 409  | Duplicate label.                |

---

#### Delete Court

Deletes a court with safety checks. Fails if the court has active or future bookings.

```
DELETE /api/v1/sites/{siteId}/courts/{courtId}
```

**Authorization:** RequireSiteManagement

**Path Parameters:**

| Parameter | Type | Description              |
|-----------|------|--------------------------|
| `siteId`  | guid | Identifier of the site.  |
| `courtId` | guid | Identifier of the court. |

**Response:** `204 No Content`

**Status Codes:**

| Code | Description                                          |
|------|------------------------------------------------------|
| 204  | Court successfully deleted.                          |
| 400  | Invalid request.                                     |
| 401  | User is not authenticated.                           |
| 403  | User does not have management permissions.           |
| 404  | Court not found.                                     |
| 409  | Cannot delete court with active or future bookings.  |

---

#### Get Site Schedule

Returns the complete schedule configuration for a site, including regular schedules and closures.

```
GET /api/v1/sites/{siteId}/schedules
```

**Authorization:** RequireSiteAccess

**Path Parameters:**

| Parameter | Type | Description             |
|-----------|------|-------------------------|
| `siteId`  | guid | Identifier of the site. |

**Response:** `200 OK`

```json
{
  "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "siteName": "PadTime Brussels South",
  "timezone": "Europe/Brussels",
  "schedules": [
    {
      "scheduleId": "d1e2f3a4-b5c6-7890-1234-567890abcdef",
      "name": "Winter Hours",
      "validFrom": "2025-10-01",
      "validUntil": "2026-03-31",
      "openingTime": "08:00",
      "closingTime": "21:00",
      "applicableDays": [1, 2, 3, 4, 5],
      "priority": 10,
      "isActive": true,
      "createdAtUtc": "2025-09-15T10:00:00Z",
      "updatedAtUtc": null
    },
    {
      "scheduleId": "e2f3a4b5-c6d7-8901-2345-6789abcdef01",
      "name": "Weekend Hours",
      "validFrom": "2025-10-01",
      "validUntil": "2026-03-31",
      "openingTime": "09:00",
      "closingTime": "20:00",
      "applicableDays": [6, 0],
      "priority": 10,
      "isActive": true,
      "createdAtUtc": "2025-09-15T10:05:00Z",
      "updatedAtUtc": null
    }
  ],
  "closures": [
    {
      "closureId": "a9b8c7d6-e5f4-3210-fedc-ba0987654321",
      "type": "Period",
      "reason": "Vacation",
      "description": "Christmas break",
      "startDate": "2026-12-24",
      "endDate": "2027-01-02",
      "modifiedOpeningTime": null,
      "modifiedClosingTime": null,
      "affectedCourtIds": null,
      "createdAtUtc": "2026-11-01T08:00:00Z",
      "updatedAtUtc": null
    }
  ]
}
```

**Status Codes:**

| Code | Description                       |
|------|-----------------------------------|
| 200  | Schedule successfully retrieved.  |
| 401  | User is not authenticated.        |
| 403  | User does not have access to this site. |
| 404  | Site not found.                   |

---

#### Create Site Schedule

Creates a new schedule rule for a site.

```
POST /api/v1/sites/{siteId}/schedules
```

**Authorization:** RequireSiteManagement

**Path Parameters:**

| Parameter | Type | Description             |
|-----------|------|-------------------------|
| `siteId`  | guid | Identifier of the site. |

**Request Body:**

```json
{
  "name": "Summer Hours",
  "validFrom": "2026-04-01",
  "validUntil": "2026-09-30",
  "openingTime": "07:00",
  "closingTime": "22:00",
  "applicableDays": [1, 2, 3, 4, 5, 6, 0],
  "priority": 10
}
```

| Field            | Type       | Required | Description                                              |
|------------------|------------|----------|----------------------------------------------------------|
| `name`           | string     | Yes      | Display name for the schedule.                           |
| `validFrom`      | date       | Yes      | Start date of validity (inclusive).                      |
| `validUntil`     | date       | No       | End date of validity (inclusive). Null for open-ended.   |
| `openingTime`    | time       | Yes      | Daily opening time (e.g., `"07:00"`).                    |
| `closingTime`    | time       | Yes      | Daily closing time (e.g., `"22:00"`).                    |
| `applicableDays` | int[]      | No       | Days of week (0=Sunday, 1=Monday ... 6=Saturday). Null for all days. |
| `priority`       | int        | Yes      | Priority for conflict resolution (higher wins).          |

**Response:** `201 Created`

```json
{
  "scheduleId": "d1e2f3a4-b5c6-7890-1234-567890abcdef"
}
```

**Headers:**

```
Location: /api/v1/sites/f47ac10b-58cc-4372-a567-0e02b2c3d479/schedules/d1e2f3a4-b5c6-7890-1234-567890abcdef
```

**Status Codes:**

| Code | Description                              |
|------|------------------------------------------|
| 201  | Schedule successfully created.           |
| 400  | Invalid request (e.g., invalid time range).|
| 401  | User is not authenticated.               |
| 403  | User does not have management permissions.|
| 404  | Site not found.                          |
| 409  | Schedule conflicts with existing schedule.|

---

#### Update Site Schedule

Updates an existing site schedule.

```
PUT /api/v1/sites/{siteId}/schedules/{scheduleId}
```

**Authorization:** RequireSiteManagement

**Path Parameters:**

| Parameter    | Type | Description                 |
|--------------|------|-----------------------------|
| `siteId`     | guid | Identifier of the site.     |
| `scheduleId` | guid | Identifier of the schedule. |

**Request Body:**

```json
{
  "name": "Summer Hours - Extended",
  "validFrom": "2026-04-01",
  "validUntil": "2026-10-15",
  "openingTime": "06:30",
  "closingTime": "22:30",
  "applicableDays": [1, 2, 3, 4, 5, 6, 0],
  "priority": 10
}
```

| Field            | Type   | Required | Description                                              |
|------------------|--------|----------|----------------------------------------------------------|
| `name`           | string | Yes      | Display name for the schedule.                           |
| `validFrom`      | date   | Yes      | Start date of validity (inclusive).                      |
| `validUntil`     | date   | No       | End date of validity (inclusive). Null for open-ended.   |
| `openingTime`    | time   | Yes      | Daily opening time.                                      |
| `closingTime`    | time   | Yes      | Daily closing time.                                      |
| `applicableDays` | int[]  | No       | Days of week. Null for all days.                         |
| `priority`       | int    | Yes      | Priority for conflict resolution (higher wins).          |

**Response:** `204 No Content`

**Status Codes:**

| Code | Description                              |
|------|------------------------------------------|
| 204  | Schedule successfully updated.           |
| 400  | Invalid request (e.g., invalid time range).|
| 401  | User is not authenticated.               |
| 403  | User does not have management permissions.|
| 404  | Site or schedule not found.              |

---

#### Delete Site Schedule

Deletes a site schedule.

```
DELETE /api/v1/sites/{siteId}/schedules/{scheduleId}
```

**Authorization:** RequireSiteManagement

**Path Parameters:**

| Parameter    | Type | Description                 |
|--------------|------|-----------------------------|
| `siteId`     | guid | Identifier of the site.     |
| `scheduleId` | guid | Identifier of the schedule. |

**Response:** `204 No Content`

**Status Codes:**

| Code | Description                     |
|------|---------------------------------|
| 204  | Schedule successfully deleted.  |
| 401  | User is not authenticated.      |
| 403  | User does not have management permissions. |
| 404  | Site or schedule not found.     |

---

#### Add Site Closure

Adds a closure (holiday, maintenance, reduced hours) to a site schedule.

```
POST /api/v1/sites/{siteId}/closures
```

**Authorization:** RequireSiteManagement

**Path Parameters:**

| Parameter | Type | Description             |
|-----------|------|-------------------------|
| `siteId`  | guid | Identifier of the site. |

**Request Body (full day closure):**

```json
{
  "type": "FullDay",
  "reason": "PublicHoliday",
  "description": "Belgian National Day",
  "startDate": "2026-07-21",
  "endDate": null,
  "modifiedOpeningTime": null,
  "modifiedClosingTime": null,
  "affectedCourtIds": null
}
```

**Request Body (period closure):**

```json
{
  "type": "Period",
  "reason": "Vacation",
  "description": "Annual Christmas closure",
  "startDate": "2026-12-24",
  "endDate": "2027-01-02",
  "modifiedOpeningTime": null,
  "modifiedClosingTime": null,
  "affectedCourtIds": null
}
```

**Request Body (reduced hours, specific courts):**

```json
{
  "type": "ReducedHours",
  "reason": "Maintenance",
  "description": "Court resurfacing",
  "startDate": "2026-05-10",
  "endDate": null,
  "modifiedOpeningTime": "10:00",
  "modifiedClosingTime": "16:00",
  "affectedCourtIds": ["7c9e6679-7425-40de-944b-e07fc1f90ae7"]
}
```

| Field                  | Type     | Required | Description                                                                 |
|------------------------|----------|----------|-----------------------------------------------------------------------------|
| `type`                 | string   | Yes      | `FullDay`, `Period`, or `ReducedHours`. See [ClosureType](#closuretype).    |
| `reason`               | string   | Yes      | Reason for the closure. See [ClosureReason](#closurereason).                |
| `description`          | string   | No       | Human-readable description.                                                 |
| `startDate`            | date     | Yes      | Start date of the closure (inclusive).                                      |
| `endDate`              | date     | No       | End date (inclusive). Required for `Period` type. Null for single-day types. |
| `modifiedOpeningTime`  | time     | No       | Modified opening time. Required for `ReducedHours` type.                    |
| `modifiedClosingTime`  | time     | No       | Modified closing time. Required for `ReducedHours` type.                    |
| `affectedCourtIds`     | guid[]   | No       | Specific courts affected. Null means all courts.                            |

**Response:** `201 Created`

```json
{
  "closureId": "a9b8c7d6-e5f4-3210-fedc-ba0987654321"
}
```

**Headers:**

```
Location: /api/v1/sites/f47ac10b-58cc-4372-a567-0e02b2c3d479/closures/a9b8c7d6-e5f4-3210-fedc-ba0987654321
```

**Status Codes:**

| Code | Description                                     |
|------|-------------------------------------------------|
| 201  | Closure successfully added.                     |
| 400  | Invalid request (e.g., invalid closure config). |
| 401  | User is not authenticated.                      |
| 403  | User does not have management permissions.      |
| 404  | Site not found.                                 |
| 409  | Closure conflicts with existing bookings.       |

---

#### Remove Site Closure

Removes a closure from a site schedule.

```
DELETE /api/v1/sites/{siteId}/closures/{closureId}
```

**Authorization:** RequireSiteManagement

**Path Parameters:**

| Parameter   | Type | Description                 |
|-------------|------|-----------------------------|
| `siteId`    | guid | Identifier of the site.     |
| `closureId` | guid | Identifier of the closure.  |

**Response:** `204 No Content`

**Status Codes:**

| Code | Description                     |
|------|---------------------------------|
| 204  | Closure successfully removed.   |
| 400  | Invalid request.                |
| 401  | User is not authenticated.      |
| 403  | User does not have management permissions. |
| 404  | Site or closure not found.      |

---

### Admin

Base path: `/api/v1/admin`
Default authorization: `RequireAdmin`

All endpoints in this section require the `RequireAdmin` policy (site admin or global admin).

---

#### Get Site Overview

Returns operational alerts for a site: unprocessed J-1 matches, unpaid participants in upcoming matches, and active organizer debts.

```
GET /api/v1/admin/sites/{siteId}/overview
```

**Authorization:** RequireAdmin

**Path Parameters:**

| Parameter | Type | Description             |
|-----------|------|-------------------------|
| `siteId`  | guid | Identifier of the site. |

**Response:** `200 OK`

```json
{
  "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "alerts": [
    {
      "type": "UnprocessedMatch",
      "description": "Match tomorrow at 14:00 on Court Central has 2 unpaid participants.",
      "payload": {
        "matchId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "unpaidCount": 2
      }
    },
    {
      "type": "OrganizerDebt",
      "description": "Member G0012 has an outstanding debt of 24.00 EUR.",
      "payload": {
        "memberId": "c3e5a7b9-2345-6789-abcd-ef0123456789",
        "debtAmountCents": 2400
      }
    }
  ]
}
```

**Status Codes:**

| Code | Description                          |
|------|--------------------------------------|
| 200  | Overview successfully retrieved.     |
| 401  | User is not authenticated.           |
| 403  | Admin does not have access to this site. |
| 404  | Site not found.                      |

---

#### Get Revenue Analytics

Returns aggregated revenue for a site within a date range, grouped by day and site. Site admins are automatically restricted to their own site.

```
GET /api/v1/admin/analytics/revenue
```

**Authorization:** RequireAdmin

**Query Parameters:**

| Parameter | Type     | Required | Description                                                     |
|-----------|----------|----------|-----------------------------------------------------------------|
| `siteId`  | guid     | No       | Optional site filter (ignored for site admins, enforced server-side). |
| `from`    | datetime | Yes      | Start of the date range (UTC).                                  |
| `to`      | datetime | Yes      | End of the date range (UTC). Must be after `from`.              |

**Response:** `200 OK`

```json
{
  "from": "2026-03-01T00:00:00Z",
  "to": "2026-03-31T23:59:59Z",
  "currency": "EUR",
  "items": [
    {
      "date": "2026-03-01",
      "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
      "amountCents": 9600,
      "paymentCount": 8
    },
    {
      "date": "2026-03-02",
      "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
      "amountCents": 7200,
      "paymentCount": 6
    }
  ]
}
```

**Status Codes:**

| Code | Description                           |
|------|---------------------------------------|
| 200  | Revenue analytics successfully retrieved.|
| 400  | Invalid date range (`from` must be before `to`). |
| 401  | User is not authenticated.            |
| 403  | User is not an admin.                 |

---

#### List Members

Returns a paginated list of members with optional filters.

```
GET /api/v1/admin/members
```

**Authorization:** RequireAdmin

**Query Parameters:**

| Parameter  | Type   | Default    | Description                                      |
|------------|--------|------------|--------------------------------------------------|
| `page`     | int    | 1          | Page number.                                     |
| `pageSize` | int    | 20         | Results per page.                                |
| `category` | string | _optional_ | Filter by member category (`Global`, `Site`, `Free`). |
| `isActive` | bool   | _optional_ | Filter by active status.                         |
| `search`   | string | _optional_ | Search by matricule or subject.                  |

**Response:** `200 OK`

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "subject": "auth0|abc123def456",
      "matricule": "G0001",
      "category": "Global",
      "siteId": null,
      "siteName": null,
      "isActive": true,
      "createdAtUtc": "2025-01-15T08:00:00Z",
      "matchCount": 47,
      "debtAmountCents": 0
    },
    {
      "id": "b2d4f6a8-1234-5678-9abc-def012345678",
      "subject": "auth0|xyz789uvw012",
      "matricule": "S00042",
      "category": "Site",
      "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
      "siteName": "PadTime Brussels South",
      "isActive": true,
      "createdAtUtc": "2025-03-20T14:30:00Z",
      "matchCount": 23,
      "debtAmountCents": 2400
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 156,
  "totalPages": 8,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

**Status Codes:**

| Code | Description                     |
|------|---------------------------------|
| 200  | Members successfully retrieved. |
| 401  | User is not authenticated.      |
| 403  | User is not an admin.           |

---

#### Get Member Detail

Returns detailed information about a specific member, including match history.

```
GET /api/v1/admin/members/{memberId}
```

**Authorization:** RequireAdmin

**Path Parameters:**

| Parameter  | Type | Description               |
|------------|------|---------------------------|
| `memberId` | guid | Identifier of the member. |

**Response:** `200 OK`

```json
{
  "id": "b2d4f6a8-1234-5678-9abc-def012345678",
  "subject": "auth0|xyz789uvw012",
  "matricule": "S00042",
  "category": "Site",
  "siteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "siteName": "PadTime Brussels South",
  "isActive": true,
  "createdAtUtc": "2025-03-20T14:30:00Z",
  "matchCount": 23,
  "debtAmountCents": 2400,
  "totalMatchesOrganized": 8,
  "totalMatchesPlayed": 23,
  "recentMatches": [
    {
      "matchId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "startAtUtc": "2026-03-28T14:00:00Z",
      "endAtUtc": "2026-03-28T15:30:00Z",
      "status": "Completed",
      "isOrganizer": false
    },
    {
      "matchId": "f6a8b0c2-d4e6-7890-1234-56789abcdef0",
      "startAtUtc": "2026-03-25T18:00:00Z",
      "endAtUtc": "2026-03-25T19:30:00Z",
      "status": "Completed",
      "isOrganizer": true
    }
  ]
}
```

**Status Codes:**

| Code | Description                     |
|------|---------------------------------|
| 200  | Member detail retrieved.        |
| 401  | User is not authenticated.      |
| 403  | User is not an admin.           |
| 404  | Member not found.               |

---

#### Activate Member

Activates a member account.

```
POST /api/v1/admin/members/{memberId}/activate
```

**Authorization:** RequireAdmin

**Path Parameters:**

| Parameter  | Type | Description               |
|------------|------|---------------------------|
| `memberId` | guid | Identifier of the member. |

**Request Body:** _None_

**Response:** `200 OK`

**Status Codes:**

| Code | Description                     |
|------|---------------------------------|
| 200  | Member successfully activated.  |
| 401  | User is not authenticated.      |
| 403  | User is not an admin.           |
| 404  | Member not found.               |

---

#### Deactivate Member

Deactivates a member account.

```
POST /api/v1/admin/members/{memberId}/deactivate
```

**Authorization:** RequireAdmin

**Path Parameters:**

| Parameter  | Type | Description               |
|------------|------|---------------------------|
| `memberId` | guid | Identifier of the member. |

**Request Body:** _None_

**Response:** `200 OK`

**Status Codes:**

| Code | Description                       |
|------|-----------------------------------|
| 200  | Member successfully deactivated.  |
| 401  | User is not authenticated.        |
| 403  | User is not an admin.             |
| 404  | Member not found.                 |

---

### Me

Base path: `/api/v1/me`
Default authorization: `RequireUser`

---

#### Get Current User Profile

Returns the current authenticated user's profile, extracted from JWT claims.

```
GET /api/v1/me
```

**Authorization:** RequireUser

**Response:** `200 OK`

```json
{
  "subject": "auth0|abc123def456",
  "matricule": "G0001",
  "category": "global",
  "role": "user",
  "siteId": null
}
```

| Field       | Type   | Nullable | Description                                           |
|-------------|--------|----------|-------------------------------------------------------|
| `subject`   | string | No       | Unique identifier from the identity provider.         |
| `matricule` | string | No       | Member matricule.                                     |
| `category`  | string | No       | Member category: `global`, `site`, or `free`.         |
| `role`      | string | No       | User role: `user`, `site_admin`, or `global_admin`.   |
| `siteId`    | guid   | Yes      | Assigned site ID (null for global and free members).  |

**Status Codes:**

| Code | Description                  |
|------|------------------------------|
| 200  | Profile successfully returned.|
| 401  | User is not authenticated.   |

---

## Error Responses

All errors follow the [RFC 7807 Problem Details](https://datatracker.ietf.org/doc/html/rfc7807) format and are returned with `Content-Type: application/problem+json`.

### Error Format

```json
{
  "type": "booking.slot_conflict",
  "title": "Conflict",
  "detail": "This time slot is already booked.",
  "status": 409
}
```

| Field    | Type   | Description                                              |
|----------|--------|----------------------------------------------------------|
| `type`   | string | Machine-readable error code (format: `context.error_name`). |
| `title`  | string | HTTP status category (`Bad Request`, `Forbidden`, `Not Found`, `Conflict`). |
| `detail` | string | Human-readable error description.                        |
| `status` | int    | HTTP status code.                                        |

### Error Code Reference

#### Booking Errors

| Code                              | Status | Description                                                 |
|-----------------------------------|--------|-------------------------------------------------------------|
| `booking.slot_conflict`           | 409    | The requested time slot is already booked.                  |
| `booking.reservation_window_denied`| 403   | Member category does not allow booking this far in advance. |
| `booking.site_scope_violation`    | 403    | Site members can only book at their assigned site.          |
| `booking.match_not_found`         | 404    | Match not found.                                            |
| `booking.match_not_public`        | 403    | The match is not a public match.                            |
| `booking.match_not_private`       | 400    | The match is not a private match.                           |
| `booking.match_full`              | 409    | The match already has 4 paid participants.                  |
| `booking.match_locked`            | 403    | The match is locked (start time reached).                   |
| `booking.already_participant`     | 409    | User is already a participant in this match.                |
| `booking.not_participant`         | 404    | User is not a participant in this match.                    |
| `booking.not_organizer`           | 403    | Only the organizer can perform this action.                 |
| `booking.invalid_transition`      | 400    | The requested state transition is not allowed.              |

#### Billing Errors

| Code                              | Status | Description                                                 |
|-----------------------------------|--------|-------------------------------------------------------------|
| `billing.organizer_debt_block`    | 403    | Organizer has outstanding debt and cannot create matches.   |
| `billing.payment_not_found`       | 404    | Payment not found.                                          |
| `billing.idempotency_conflict`    | 409    | A payment with this idempotency key already exists.         |
| `billing.payment_already_processed`| 403   | The payment has already been processed.                     |
| `billing.invalid_amount`          | 400    | Payment amount must be positive.                            |

#### Member Errors

| Code                              | Status | Description                                                 |
|-----------------------------------|--------|-------------------------------------------------------------|
| `member.not_found`                | 404    | Member not found.                                           |
| `member.invalid_matricule`        | 400    | Invalid matricule format.                                   |
| `member.inactive`                 | 403    | Member account is inactive.                                 |

#### Site Errors

| Code                                      | Status | Description                                           |
|-------------------------------------------|--------|-------------------------------------------------------|
| `site.not_found`                          | 404    | Site not found.                                       |
| `site.closed`                             | 400    | The site is closed on the requested date.             |
| `site.invalid_schedule`                   | 400    | Invalid schedule configuration.                       |
| `site.invalid_closure`                    | 400    | Invalid closure configuration.                        |
| `site.schedule_conflict`                  | 409    | Schedule conflicts with an existing schedule.         |
| `site.closure_conflicts_with_bookings`    | 409    | Closure conflicts with existing bookings.             |
| `site.cannot_delete_with_active_bookings` | 409    | Cannot delete site with active or future bookings.    |
| `site.already_deactivated`                | 409    | Site is already deactivated.                          |
| `site.already_active`                     | 409    | Site is already active.                               |
| `site.closure_not_found`                  | 404    | Closure not found.                                    |
| `site.schedule_not_found`                 | 404    | Schedule not found.                                   |

#### Schedule Errors

| Code                                | Status | Description                                 |
|-------------------------------------|--------|---------------------------------------------|
| `site_schedule.invalid_date_range`  | 400    | Schedule end date must be after start date. |
| `site_schedule.invalid_time_range`  | 400    | Closing time must be after opening time.    |

#### Court Errors

| Code                                        | Status | Description                                             |
|---------------------------------------------|--------|---------------------------------------------------------|
| `court.not_found`                           | 404    | Court not found.                                        |
| `court.inactive`                            | 400    | The court is not active.                                |
| `court.duplicate_label`                     | 409    | A court with this label already exists for this site.   |
| `court.cannot_delete_with_active_bookings`  | 409    | Cannot delete court with active or future bookings.     |

---

## Enums

### MatchStatus

State machine for match lifecycle. Transitions are enforced by the domain model.

| Value       | Int | Description                                                    |
|-------------|-----|----------------------------------------------------------------|
| `Draft`     | 0   | Initial state (not used in practice, transitions immediately). |
| `Private`   | 1   | Private match -- participants added by organizer.              |
| `Public`    | 2   | Public match -- open for anyone to join (first paid, first served). |
| `Full`      | 3   | Match is full (4 paid participants).                           |
| `Locked`    | 4   | Match start time reached -- no more changes allowed.           |
| `Completed` | 5   | Match end time reached -- completed successfully.              |
| `Cancelled` | 6   | Match cancelled by organizer or admin.                         |

### PadMatchType

Determines join rules for a match.

| Value     | Int | Description                                      |
|-----------|-----|--------------------------------------------------|
| `Private` | 1   | Participants are added manually by the organizer. |
| `Public`  | 2   | Anyone can join by paying.                        |

### PaymentStatus

Payment status for a participant's slot.

| Value      | Int | Description                                          |
|------------|-----|------------------------------------------------------|
| `Unpaid`   | 0   | Payment has not been initiated.                      |
| `Pending`  | 1   | Payment initiated, waiting for confirmation.         |
| `Paid`     | 2   | Payment confirmed.                                   |
| `Failed`   | 3   | Payment failed.                                      |
| `Excluded` | 4   | Participant excluded (unpaid at J-1 deadline).       |

### MemberCategory

Determines booking window and site restrictions.

| Value    | Int | Description                                                    |
|----------|-----|----------------------------------------------------------------|
| `Global` | 1   | Global member (Gxxxx) -- Can book J-21, all sites.             |
| `Site`   | 2   | Site member (Sxxxxx) -- Can book J-14, restricted to assigned site. |
| `Free`   | 3   | Free member (Lxxxxx) -- Can book J-5, all sites.               |

### ClosureType

Type of site closure.

| Value          | Int | Description                              |
|----------------|-----|------------------------------------------|
| `FullDay`      | 1   | Full day closure.                        |
| `Period`       | 2   | Period closure (multiple consecutive days).|
| `ReducedHours` | 3   | Reduced hours for a specific day.        |

### ClosureReason

Reason for a site closure (for categorization and reporting).

| Value          | Int | Description                   |
|----------------|-----|-------------------------------|
| `PublicHoliday`| 1   | Public holiday.               |
| `Vacation`     | 2   | Vacation (annual leave).      |
| `Maintenance`  | 3   | Maintenance or repair work.   |
| `PrivateEvent` | 4   | Private event.                |
| `Weather`      | 5   | Adverse weather conditions.   |
| `Other`        | 99  | Other reason.                 |
