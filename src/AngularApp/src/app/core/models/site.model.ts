// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
// ========================================
// CORE TYPES
// ========================================

/** A padel site (club) with its address, timezone, and associated courts. */
export interface Site {
  siteId: string;
  name: string;
  streetNumber: string;
  street: string;
  postcode: string;
  city: string;
  country: string;
  timezone: string;
  isActive: boolean;
  createdAtUtc: string;
  courtCount: number;
  courts: CourtSummary[];
}

/** Lightweight court representation used in site listings. */
export interface CourtSummary {
  courtId: string;
  label: string;
  isActive: boolean;
}

/** Full site detail including courts, schedules, and closures. */
export interface SiteDetail {
  siteId: string;
  name: string;
  streetNumber: string;
  street: string;
  postcode: string;
  city: string;
  country: string;
  timezone: string;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  courts: Court[];
  schedules: Schedule[];
  closures: Closure[];
}

/** A padel court belonging to a site. */
export interface Court {
  courtId: string;
  label: string;
  isActive: boolean;
  createdAtUtc: string;
}

/** An opening schedule for a site, defining operating hours on specific days. */
export interface Schedule {
  scheduleId: string;
  name: string;
  validFrom: string;
  validUntil: string | null;
  openingTime: string;
  closingTime: string;
  applicableDays: number[];
  priority: number;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

/** A temporary closure or modified schedule for a site or specific courts. */
export interface Closure {
  closureId: string;
  type: string;
  reason: string;
  description: string | null;
  startDate: string;
  endDate: string;
  modifiedOpeningTime: string | null;
  modifiedClosingTime: string | null;
  affectedCourtIds: string[] | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

// ========================================
// PAGINATION
// ========================================

/** Generic paginated result with navigation metadata. */
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// ========================================
// REQUEST DTOs
// ========================================

/** Request payload to create a new site. */
export interface CreateSiteRequest {
  name: string;
  streetNumber: string;
  street: string;
  postcode: string;
  city: string;
  country: string;
  timezone: string;
}

/** Request payload to update an existing site. */
export interface UpdateSiteRequest {
  name: string;
  streetNumber: string;
  street: string;
  postcode: string;
  city: string;
  country: string;
  timezone: string;
}

export interface CreateCourtRequest {
  label: string;
}

export interface CreateScheduleRequest {
  name: string;
  validFrom: string;
  validUntil: string | null;
  openingTime: string;
  closingTime: string;
  applicableDays: number[] | null;
  priority: number;
}

export interface CreateClosureRequest {
  type: number;
  reason: number;
  description: string | null;
  startDate: string;
  endDate: string | null;
  modifiedOpeningTime: string | null;
  modifiedClosingTime: string | null;
  affectedCourtIds: string[] | null;
}

export type CourtDetailDto = Court;
export type CreateCourtResponse = { courtId: string };
export type CreateScheduleResponse = { scheduleId: string };
export type CreateClosureResponse = { closureId: string };