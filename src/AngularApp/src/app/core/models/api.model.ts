// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
/** RFC 7807 Problem Details response structure for API errors. */
export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

/** Generic paginated response envelope for list API endpoints. */
export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

/** Well-known API error type codes used for client-side error handling. */
export const ErrorCodes = {
  SLOT_CONFLICT: 'booking.slot_conflict',
  RESERVATION_WINDOW_DENIED: 'booking.reservation_window_denied',
  SITE_SCOPE_VIOLATION: 'booking.site_scope_violation',
  ORGANIZER_DEBT_BLOCK: 'billing.organizer_debt_block',
  MATCH_NOT_PUBLIC: 'match.not_public',
  MATCH_FULL: 'match.full',
  IDEMPOTENCY_CONFLICT: 'payment.idempotency_conflict',
} as const;