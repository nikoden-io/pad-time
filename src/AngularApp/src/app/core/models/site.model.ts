// ========================================
// CORE TYPES
// ========================================

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

export interface CourtSummary {
  courtId: string;
  label: string;
  isActive: boolean;
}

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

export interface Court {
  courtId: string;
  label: string;
  isActive: boolean;
  createdAtUtc: string;
}

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

export interface CreateSiteRequest {
  name: string;
  streetNumber: string;
  street: string;
  postcode: string;
  city: string;
  country: string;
  timezone: string;
}

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
