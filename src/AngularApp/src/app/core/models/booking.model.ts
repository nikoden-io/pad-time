// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
/** Whether a match is open to the public or restricted to invited players. */
export type MatchType = 'private' | 'public';
/** Lifecycle status of a match from creation through completion. */
export type MatchStatus = 'draft' | 'private' | 'public' | 'full' | 'locked' | 'completed' | 'cancelled';
/** Payment status for a participant in a match. */
export type PaymentStatus = 'unpaid' | 'pending' | 'paid' | 'failed' | 'excluded';
/** Role of a participant within a match. */
export type ParticipantRole = 'organizer' | 'player';

/** A bookable time window with availability indication. */
export interface TimeSlot {
  startAt: string;
  endAt: string;
  available: boolean;
}

/** A player enrolled in a match, with their role and payment status. */
export interface Participant {
  memberId: string;
  matricule: string;
  role: ParticipantRole;
  paymentStatus: PaymentStatus;
}

/** Response containing court availability slots for a given site and date. */
export type AvailabilityResponse = {
  siteId: string;
  date: string; // yyyy-mm-dd
  slots: AvailabilitySlot[];
};

/** A single availability slot for a specific court with start/end times in UTC. */
export type AvailabilitySlot = {
  courtId: string;
  courtLabel: string;
  startAt: string; // utc iso (z)
  endAt: string;   // utc iso (z)
  available: boolean;
};

/** A padel match with its schedule, participants, and pricing. */
export interface Match {
  matchId: string;
  siteId: string;
  courtId: string;
  startAtUtc: string;
  endAtUtc: string;
  type: MatchType;
  status: MatchStatus;
  organizerId: string;
  participants: Participant[];
  priceTotalCents: number;
}

/** Request payload to create a new match on a specific court and time slot. */
export interface CreateMatchRequest {
  siteId: string;
  courtId: string;
  startAt: string;
  type: MatchType;
  privateParticipantsMatricules?: string[];
}

/** Response returned after successfully creating a match. */
export interface CreateMatchResponse {
  matchId: string;
}

/** Request payload to join an existing public match. */
export interface JoinMatchRequest {
  idempotencyKey: string;
}

/** Response returned after joining a match, including the resulting payment. */
export interface JoinMatchResponse {
  paymentId: string;
  status: PaymentStatus;
}

/** Query parameters for listing matches with scope, date range, and pagination. */
export interface MatchListParams {
  scope: 'public' | 'mine' | 'site';
  siteId?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}

/** Request payload to create a court reservation. */
export interface CreateReservationRequest {
  siteId: string;
  courtId: string;
  startAt: string;
  type: MatchType;
  privateParticipantsMatricules?: string[];
}

/** Response returned after successfully creating a reservation. */
export interface CreateReservationResponse {
  reservationId: string;
}