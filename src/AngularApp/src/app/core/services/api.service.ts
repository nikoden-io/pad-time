// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import {Injectable, inject} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {environment} from '@env/environment';
import {
  CurrentUser,
  Site,
  Court,
  Match,
  CreateMatchRequest,
  CreateMatchResponse,
  CreateReservationRequest,
  CreateReservationResponse,
  JoinMatchRequest,
  SlotSuggestionsResponse,
  JoinMatchResponse,
  MatchListParams,
  PaginatedResponse,
  Payment, AvailabilityResponse,
  SiteOverview,
  RevenueAnalytics,
  AdminMember,
  AdminMemberDetail,
  AiTrendsResponse,
} from '../models';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  // Identity
  getMe(): Observable<CurrentUser> {
    return this.http.get<CurrentUser>(`${this.baseUrl}/me`);
  }

  // Sites & Courts
  getSites(): Observable<Site[]> {
    return this.http.get<Site[]>(`${this.baseUrl}/sites`);
  }

  getCourts(siteId: string): Observable<Court[]> {
    return this.http.get<Court[]>(`${this.baseUrl}/sites/${siteId}/courts`);
  }

  // Availability
  getAvailability(args: {
    siteId: string;
    date: string;
    courtId?: string;
  }): Observable<AvailabilityResponse> {

    let params = new HttpParams()
      .set('siteId', args.siteId)
      .set('date', args.date);

    if (args.courtId !== undefined) {
      params = params.set('courtId', args.courtId);
    }

    return this.http.get<AvailabilityResponse>(
      `${this.baseUrl}/availability`,
      {params}
    );
  }

  // Matches
  getMatches(params: MatchListParams): Observable<PaginatedResponse<Match>> {
    let httpParams = new HttpParams().set('scope', params.scope);

    if (params.siteId) httpParams = httpParams.set('siteId', params.siteId);
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    if (params.page) httpParams = httpParams.set('page', params.page.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<PaginatedResponse<Match>>(`${this.baseUrl}/matches`, {params: httpParams});
  }

  getPublicMatches(params?: {
    siteId?: string;
    fromUtc?: string;
    toUtc?: string;
    page?: number;
    pageSize?: number;
  }): Observable<Match[]> {
    let httpParams = new HttpParams();

    if (params?.siteId) httpParams = httpParams.set('siteId', params.siteId);
    if (params?.fromUtc) httpParams = httpParams.set('fromUtc', params.fromUtc);
    if (params?.toUtc) httpParams = httpParams.set('toUtc', params.toUtc);
    if (params?.page) httpParams = httpParams.set('page', params.page.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<Match[]>(`${this.baseUrl}/matches/public`, {params: httpParams});
  }

  getUserMatches(): Observable<PaginatedResponse<Match>> {
    return this.http.get<PaginatedResponse<Match>>(`${this.baseUrl}/matches/user`);
  }

  getMatch(matchId: string): Observable<Match> {
    return this.http.get<Match>(`${this.baseUrl}/matches/${matchId}`);
  }

  createMatch(request: CreateMatchRequest): Observable<CreateMatchResponse> {
    return this.http.post<CreateMatchResponse>(`${this.baseUrl}/matches`, request);
  }

  joinMatch(matchId: string, request: JoinMatchRequest): Observable<JoinMatchResponse> {
    return this.http.post<JoinMatchResponse>(`${this.baseUrl}/matches/${matchId}/join`, request);
  }

  cancelMatch(matchId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/matches/${matchId}/cancel`, {});
  }

  getSlotSuggestions(): Observable<SlotSuggestionsResponse> {
    return this.http.get<SlotSuggestionsResponse>(`${this.baseUrl}/matches/suggestions`);
  }

  // Reservations
  createReservation(request: CreateReservationRequest): Observable<CreateReservationResponse> {
    return this.http.post<CreateReservationResponse>(`${this.baseUrl}/reservations`, request);
  }

  // Payments
  getPayment(paymentId: string): Observable<Payment> {
    return this.http.get<Payment>(`${this.baseUrl}/payments/${paymentId}`);
  }

  payMatch(matchId: string, idempotencyKey: string): Observable<{paymentId: string; status: string}> {
    return this.http.post<{paymentId: string; status: string}>(
      `${this.baseUrl}/payments/matches/${matchId}/pay`,
      {idempotencyKey}
    );
  }

  // Admin
  getAdminOverview(siteId: string): Observable<SiteOverview> {
    return this.http.get<SiteOverview>(`${this.baseUrl}/admin/sites/${siteId}/overview`);
  }

  getAdminRevenue(params: {siteId?: string; from: string; to: string}): Observable<RevenueAnalytics> {
    let httpParams = new HttpParams()
      .set('from', params.from)
      .set('to', params.to);
    if (params.siteId) httpParams = httpParams.set('siteId', params.siteId);
    return this.http.get<RevenueAnalytics>(`${this.baseUrl}/admin/analytics/revenue`, {params: httpParams});
  }

  // Members
  getAdminMembers(params?: {
    page?: number; pageSize?: number;
    category?: string; isActive?: boolean; search?: string;
  }): Observable<PaginatedResponse<AdminMember>> {
    let httpParams = new HttpParams();
    if (params?.page) httpParams = httpParams.set('page', params.page.toString());
    if (params?.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    if (params?.category) httpParams = httpParams.set('category', params.category);
    if (params?.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
    if (params?.search) httpParams = httpParams.set('search', params.search);
    return this.http.get<PaginatedResponse<AdminMember>>(`${this.baseUrl}/admin/members`, {params: httpParams});
  }

  getAdminMemberDetail(memberId: string): Observable<AdminMemberDetail> {
    return this.http.get<AdminMemberDetail>(`${this.baseUrl}/admin/members/${memberId}`);
  }

  activateMember(memberId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/admin/members/${memberId}/activate`, {});
  }

  deactivateMember(memberId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/admin/members/${memberId}/deactivate`, {});
  }

  // AI Trends
  getAiTrends(siteId?: string): Observable<AiTrendsResponse> {
    let params = new HttpParams();
    if (siteId) params = params.set('siteId', siteId);
    return this.http.get<AiTrendsResponse>(`${this.baseUrl}/admin/analytics/ai-trends`, {params});
  }
}