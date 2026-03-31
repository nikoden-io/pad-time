// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
/** Lifecycle state of a payment transaction. */
export type PaymentState = 'pending' | 'paid' | 'failed' | 'refunded';

/** A payment record linking a member to a match with an amount and status. */
export interface Payment {
  paymentId: string;
  matchId: string;
  memberId: string;
  amountCents: number;
  status: PaymentState;
  createdAt: string;
}