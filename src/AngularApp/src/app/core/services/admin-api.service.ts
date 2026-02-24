import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SiteOverview {
  siteId: string;
  alerts: Alert[];
}

export interface Alert {
  type: 'j1_incomplete' | 'unpaid' | 'debt';
  message: string;
  matchId?: string;
  memberId?: string;
}

export interface RevenueItem {
  date: string;
  amountCents: number;
}

export interface RevenueResponse {
  from: string;
  to: string;
  currency: string;
  items: RevenueItem[];
}

@Injectable({
  providedIn: 'root',
})
export class AdminApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getSiteOverview(siteId: string): Observable<SiteOverview> {
    return this.http.get<SiteOverview>(`${this.baseUrl}/admin/sites/${siteId}/overview`);
  }

  getRevenue(siteId: string | null, from: string, to: string): Observable<RevenueResponse> {
    let params = new HttpParams()
      .set('from', from)
      .set('to', to);

    if (siteId) {
      params = params.set('siteId', siteId);
    }

    return this.http.get<RevenueResponse>(`${this.baseUrl}/admin/analytics/revenue`, { params });
  }
}
