import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CurrentUser,
  Site,
  Court,
  AvailabilityResponse,
  Match,
  CreateMatchRequest,
  CreateMatchResponse,
  JoinMatchRequest,
  JoinMatchResponse,
  MatchListParams,
  PaginatedResponse,
  Payment,
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
  getAvailability(siteId: string, date: string, courtId?: string): Observable<AvailabilityResponse> {
    let params = new HttpParams()
      .set('siteId', siteId)
      .set('date', date);

    if (courtId) {
      params = params.set('courtId', courtId);
    }

    return this.http.get<AvailabilityResponse>(`${this.baseUrl}/availability`, { params });
  }

  // Matches
  getMatches(params: MatchListParams): Observable<PaginatedResponse<Match>> {
    let httpParams = new HttpParams().set('scope', params.scope);

    if (params.siteId) httpParams = httpParams.set('siteId', params.siteId);
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    if (params.page) httpParams = httpParams.set('page', params.page.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<PaginatedResponse<Match>>(`${this.baseUrl}/matches`, { params: httpParams });
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

  // Payments
  getPayment(paymentId: string): Observable<Payment> {
    return this.http.get<Payment>(`${this.baseUrl}/payments/${paymentId}`);
  }
}
