export type MatchType = 'private' | 'public';
export type MatchStatus = 'draft' | 'private' | 'public' | 'full' | 'locked' | 'completed' | 'cancelled';
export type PaymentStatus = 'unpaid' | 'pending' | 'paid' | 'failed' | 'excluded';
export type ParticipantRole = 'organizer' | 'player';

export interface TimeSlot {
  startAt: string;
  endAt: string;
  available: boolean;
}

export interface Participant {
  memberId: string;
  matricule: string;
  role: ParticipantRole;
  paymentStatus: PaymentStatus;
}

export type AvailabilityResponse = {
  siteId: string;
  date: string; // yyyy-mm-dd
  slots: AvailabilitySlot[];
};

export type AvailabilitySlot = {
  courtId: string;
  courtLabel: string;
  startAt: string; // utc iso (z)
  endAt: string;   // utc iso (z)
  available: boolean;
};

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

export interface CreateMatchRequest {
  siteId: string;
  courtId: string;
  startAt: string;
  type: MatchType;
  privateParticipantsMatricules?: string[];
}

export interface CreateMatchResponse {
  matchId: string;
}

export interface JoinMatchRequest {
  idempotencyKey: string;
}

export interface JoinMatchResponse {
  paymentId: string;
  status: PaymentStatus;
}

export interface MatchListParams {
  scope: 'public' | 'mine' | 'site';
  siteId?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateReservationRequest {
  siteId: string;
  courtId: string;
  startAt: string;
  type: MatchType;
  privateParticipantsMatricules?: string[];
}

export interface CreateReservationResponse {
  reservationId: string;
}
