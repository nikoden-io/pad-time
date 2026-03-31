// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
/** An alert raised for a site, such as unprocessed bookings or outstanding debts. */
export interface SiteAlert {
  type: 'j1_unprocessed' | 'unpaid_participants' | 'organizer_debt' | string;
  description: string;
  payload: Record<string, any> | null;
}

/** Overview of a site's operational status, including active alerts. */
export interface SiteOverview {
  siteId: string;
  alerts: SiteAlert[];
}

/** A single daily revenue data point for a specific site. */
export interface RevenueItem {
  date: string; // "yyyy-MM-dd"
  siteId: string;
  amountCents: number;
  paymentCount: number;
}

/** Aggregated revenue analytics over a date range. */
export interface RevenueAnalytics {
  from: string;
  to: string;
  currency: string;
  items: RevenueItem[];
}

// ── Members ──────────────────────────────────────────

/** Summary of a platform member as seen in the admin members list. */
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

/** Detailed member profile including match history, extending the summary. */
export interface AdminMemberDetail extends AdminMember {
  totalMatchesOrganized: number;
  totalMatchesPlayed: number;
  recentMatches: MemberMatch[];
}

/** A match associated with a member, used in the member detail view. */
export interface MemberMatch {
  matchId: string;
  startAtUtc: string;
  endAtUtc: string;
  status: string;
  isOrganizer: boolean;
}

// ── AI Trends ───────────────────────────────────────

/** Impact direction of a business trend. */
export type TrendImpact = 'positive' | 'negative' | 'neutral';

/** A single AI-generated business trend insight. */
export interface AiTrend {
  category: string;
  title: string;
  description: string;
  impact: TrendImpact;
  icon: string;
}

/** Response containing AI-generated business trends. */
export interface AiTrendsResponse {
  trends: AiTrend[];
  generatedAtUtc: string;
  fallbackUsed: boolean;
}