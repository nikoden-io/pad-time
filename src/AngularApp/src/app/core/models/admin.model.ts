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

// ── Members ──────────────────────────────────────────

export interface AdminMember {
  id: string;
  subject: string;
  matricule: string;
  category: 'Global' | 'Site' | 'Free';
  siteId: string | null;
  siteName: string | null;
  isActive: boolean;
  createdAtUtc: string;
  matchCount: number;
  debtAmountCents: number;
}

export interface AdminMemberDetail extends AdminMember {
  totalMatchesOrganized: number;
  totalMatchesPlayed: number;
  recentMatches: MemberMatch[];
}

export interface MemberMatch {
  matchId: string;
  startAtUtc: string;
  endAtUtc: string;
  status: string;
  isOrganizer: boolean;
}
