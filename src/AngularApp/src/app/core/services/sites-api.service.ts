import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';
import {
  PagedResult,
  SiteListDto,
  SiteDetailDto,
  SitesQueryParams,
  CreateSiteRequest,
  CreateSiteResponse,
  UpdateSiteRequest,
  CreateCourtRequest,
  CreateCourtResponse,
  UpdateCourtRequest,
  SiteScheduleDetailDto,
  CreateScheduleRequest,
  CreateScheduleResponse,
  UpdateScheduleRequest,
  CreateClosureRequest,
  CreateClosureResponse,
} from '../models';

@Injectable({ providedIn: 'root' })
export class SitesApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/sites`;

  // ── Sites ──────────────────────────────────────────────

  getSites(params: SitesQueryParams): Observable<PagedResult<SiteListDto>> {
    let hp = new HttpParams();
    if (params.page) hp = hp.set('page', params.page);
    if (params.pageSize) hp = hp.set('pageSize', params.pageSize);
    if (params.searchTerm) hp = hp.set('searchTerm', params.searchTerm);
    if (params.isActive !== undefined && params.isActive !== null)
      hp = hp.set('isActive', params.isActive);
    if (params.city) hp = hp.set('city', params.city);
    if (params.country) hp = hp.set('country', params.country);
    return this.http.get<PagedResult<SiteListDto>>(this.base, { params: hp });
  }

  getSite(siteId: string): Observable<SiteDetailDto> {
    return this.http.get<SiteDetailDto>(`${this.base}/${siteId}`);
  }

  createSite(req: CreateSiteRequest): Observable<CreateSiteResponse> {
    return this.http.post<CreateSiteResponse>(this.base, req);
  }

  updateSite(siteId: string, req: UpdateSiteRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${siteId}`, req);
  }

  deleteSite(siteId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${siteId}`);
  }

  activateSite(siteId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${siteId}/activate`, {});
  }

  deactivateSite(siteId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${siteId}/deactivate`, {});
  }

  // ── Courts ─────────────────────────────────────────────

  createCourt(siteId: string, req: CreateCourtRequest): Observable<CreateCourtResponse> {
    return this.http.post<CreateCourtResponse>(`${this.base}/${siteId}/courts`, req);
  }

  updateCourt(siteId: string, courtId: string, req: UpdateCourtRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${siteId}/courts/${courtId}`, req);
  }

  deleteCourt(siteId: string, courtId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${siteId}/courts/${courtId}`);
  }

  // ── Schedules ──────────────────────────────────────────

  getSchedules(siteId: string): Observable<SiteScheduleDetailDto> {
    return this.http.get<SiteScheduleDetailDto>(`${this.base}/${siteId}/schedules`);
  }

  createSchedule(siteId: string, req: CreateScheduleRequest): Observable<CreateScheduleResponse> {
    return this.http.post<CreateScheduleResponse>(`${this.base}/${siteId}/schedules`, req);
  }

  updateSchedule(
    siteId: string,
    scheduleId: string,
    req: UpdateScheduleRequest,
  ): Observable<void> {
    return this.http.put<void>(`${this.base}/${siteId}/schedules/${scheduleId}`, req);
  }

  deleteSchedule(siteId: string, scheduleId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${siteId}/schedules/${scheduleId}`);
  }

  // ── Closures ───────────────────────────────────────────

  createClosure(siteId: string, req: CreateClosureRequest): Observable<CreateClosureResponse> {
    return this.http.post<CreateClosureResponse>(`${this.base}/${siteId}/closures`, req);
  }

  deleteClosure(siteId: string, closureId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${siteId}/closures/${closureId}`);
  }
}
