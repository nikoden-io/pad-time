export interface SiteAlert {
  type: 'j1_unprocessed' | 'unpaid_participants' | 'organizer_debt' | string;
  description: string;
  payload: Record<string, any> | null;
}

export interface SiteOverview {
  siteId: string;
  alerts: SiteAlert[];
}

export interface RevenueItem {
  date: string; // "yyyy-MM-dd"
  siteId: string;
  amountCents: number;
  paymentCount: number;
}

export interface RevenueAnalytics {
  from: string;
  to: string;
  currency: string;
  items: RevenueItem[];
}
