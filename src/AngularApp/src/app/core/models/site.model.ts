// --- Existing (used by booking feature) ---

export interface Site {
  siteId: string;
  name: string;
  timezone: string;
}

export interface Court {
  courtId: string;
  label: string;
  active: boolean;
}

export interface CreateCourtRequest {
  label: string;
}

export interface CreateCourtResponse {
  courtId: string;
}

// --- Admin: Paged response ---

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// --- Admin: Site DTOs ---

export interface SiteListDto {
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
}

export interface SiteDetailDto {
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
  courts: CourtDetailDto[];
  schedules: SiteScheduleDto[];
  closures: SiteClosureDto[];
}

export interface CreateSiteRequest {
  name: string;
  streetNumber: string;
  street: string;
  postcode: string;
  city: string;
  country: string;
  timezone: string;
}

export type UpdateSiteRequest = CreateSiteRequest;

export interface CreateSiteResponse {
  siteId: string;
}

// --- Admin: Court DTOs ---

export interface CourtDetailDto {
  courtId: string;
  label: string;
  isActive: boolean;
  createdAtUtc: string;
}

export interface UpdateCourtRequest {
  label: string;
}

// --- Admin: Schedule DTOs ---

export interface SiteScheduleDto {
  scheduleId: string;
  name: string;
  validFrom: string;
  validUntil: string | null;
  openingTime: string;
  closingTime: string;
  applicableDays: number[] | null;
  priority: number;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface SiteScheduleDetailDto {
  siteId: string;
  siteName: string;
  timezone: string;
  schedules: SiteScheduleDto[];
  closures: SiteClosureDto[];
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

export type UpdateScheduleRequest = CreateScheduleRequest;

export interface CreateScheduleResponse {
  scheduleId: string;
}

// --- Admin: Closure DTOs ---

export interface SiteClosureDto {
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

export interface CreateClosureResponse {
  closureId: string;
}

// --- Admin: Query params ---

export interface SitesQueryParams {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  isActive?: boolean | null;
  city?: string;
  country?: string;
}
