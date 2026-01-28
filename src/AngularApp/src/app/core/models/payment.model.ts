export type PaymentState = 'pending' | 'paid' | 'failed' | 'refunded';

export interface Payment {
  paymentId: string;
  matchId: string;
  memberId: string;
  amountCents: number;
  status: PaymentState;
  createdAt: string;
}
