import {Injectable, inject} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable, tap, catchError, throwError} from 'rxjs';
import {
  Site,
  SiteDetail,
  Court,
  Schedule,
  Closure,
  PagedResult,
  CreateSiteRequest,
  UpdateSiteRequest,
  CreateCourtRequest,
  CreateScheduleRequest,
  CreateClosureRequest
} from '@core/models';
import {environment} from '@env/environment';
import {SitesStore} from '@features/admin/domain/store/sites.store';

@Injectable({providedIn: 'root'})
export class SitesRepository {
  private readonly http = inject(HttpClient);
  private readonly store = inject(SitesStore);
  private readonly baseUrl = `${environment.apiUrl}/sites`;

  // ========================================
  // SITES
  // ========================================

  getSites(page: number = 1, pageSize: number = 10): Observable<PagedResult<Site>> {
    this.store.setLoading('sites', true);

    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<Site>>(this.baseUrl, {params}).pipe(
      tap(result => this.store.setSites(result)),
      catchError(error => {
        this.store.setError(error.message);
        return throwError(() => error);
      })
    );
  }

  getSiteById(siteId: string): Observable<SiteDetail> {
    this.store.setLoading('siteDetail', true);

    return this.http.get<SiteDetail>(`${this.baseUrl}/${siteId}`).pipe(
      tap(site => this.store.setSelectedSite(site)),
      catchError(error => {
        this.store.setError(error.message);
        return throwError(() => error);
      })
    );
  }

  createSite(request: CreateSiteRequest): Observable<{ siteId: string }> {
    return this.http.post<{ siteId: string }>(this.baseUrl, request).pipe(
      tap(response => {
        // Optimistic update: reload list after creation
        this.getSites().subscribe();
      }),
      catchError(error => {
        this.store.setError(error.message);
        return throwError(() => error);
      })
    );
  }

  updateSite(siteId: string, request: UpdateSiteRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${siteId}`, request).pipe(
      tap(() => {
        // Optimistic update: reload site detail
        this.getSiteById(siteId).subscribe();
      }),
      catchError(error => {
        this.store.setError(error.message);
        return throwError(() => error);
      })
    );
  }

  deleteSite(siteId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${siteId}`).pipe(
      tap(() => {
        this.store.deleteSite(siteId);
      }),
      catchError(error => {
        this.store.setError(error.message);
        return throwError(() => error);
      })
    );
  }

  toggleSiteActive(siteId: string, activate: boolean): Observable<void> {
    const url = `${this.baseUrl}/${siteId}/${activate ? 'activate' : 'deactivate'}`;

    return this.http.post<void>(url, null).pipe(
      tap(() => {
        this.store.toggleSiteActive(siteId, activate);
      }),
      catchError(error => {
        this.store.setError(error.message);
        return throwError(() => error);
      })
    );
  }

  // ========================================
  // COURTS
  // ========================================

  getCourts(siteId: string): Observable<Court[]> {
    this.store.setLoading('courts', true);

    return this.http.get<Court[]>(`${this.baseUrl}/${siteId}/courts`).pipe(
      tap(courts => this.store.setCourts(courts)),
      catchError(error => {
        this.store.setError(error.message);
        return throwError(() => error);
      })
    );
  }

  createCourt(siteId: string, request: CreateCourtRequest): Observable<{ courtId: string }> {
    return this.http.post<{ courtId: string }>(`${this.baseUrl}/${siteId}/courts`, request).pipe(
      tap(() => {
        this.getCourts(siteId).subscribe();
      }),
      catchError(error => {
        this.store.setError(error.message);
        return throwError(() => error);
      })
    );
  }

  updateCourt(siteId: string, courtId: string, request: { label: string }): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${siteId}/courts/${courtId}`, request).pipe(
      tap(() => {
        this.getCourts(siteId).subscribe();
      }),
      catchError(error => {
        this.store.setError(error.message);
        return throwError(() => error);
      })
    );
  }

  deleteCourt(siteId: string, courtId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${siteId}/courts/${courtId}`).pipe(
      tap(() => {
        this.store.deleteCourt(courtId);
      }),
      catchError(error => {
        this.store.setError(error.message);
        return throwError(() => error);
      })
    );
  }

  // ========================================
  // SCHEDULES
  // ========================================

  getSchedules(siteId: string): Observable<{ schedules: Schedule[]; closures: Closure[] }> {
    this.store.setLoading('schedules', true);

    return this.http.get<{ schedules: Schedule[]; closures: Closure[] }>(
      `${this.baseUrl}/${siteId}/schedules`
    ).pipe(
      tap(result => {
        this.store.setSchedules(result.schedules);
        this.store.setClosures(result.closures);
      }),
      catchError(error => {
        this.store.setError(error.message);
        return throwError(() => error);
      })
    );
  }

  createSchedule(siteId: string, request: CreateScheduleRequest): Observable<{ scheduleId: string }> {
    return this.http.post<{ scheduleId: string }>(
      `${this.baseUrl}/${siteId}/schedules`,
      request
    ).pipe(
      tap(() => {
        this.getSchedules(siteId).subscribe();
      }),
      catchError(error => {
        this.store.setError(error.message);
        return throwError(() => error);
      })
    );
  }

  updateSchedule(siteId: string, scheduleId: string, request: CreateScheduleRequest): Observable<void> {
    return this.http.put<void>(
      `${this.baseUrl}/${siteId}/schedules/${scheduleId}`,
      request
    ).pipe(
      tap(() => {
        this.getSchedules(siteId).subscribe();
      }),
      catchError(error => {
        this.store.setError(error.message);
        return throwError(() => error);
      })
    );
  }

  deleteSchedule(siteId: string, scheduleId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${siteId}/schedules/${scheduleId}`).pipe(
      tap(() => {
        this.store.deleteSchedule(scheduleId);
      }),
      catchError(error => {
        this.store.setError(error.message);
        return throwError(() => error);
      })
    );
  }

  // ========================================
  // CLOSURES
  // ========================================

  createClosure(siteId: string, request: CreateClosureRequest): Observable<{ closureId: string }> {
    return this.http.post<{ closureId: string }>(
      `${this.baseUrl}/${siteId}/closures`,
      request
    ).pipe(
      tap(() => {
        this.getSchedules(siteId).subscribe();
      }),
      catchError(error => {
        this.store.setError(error.message);
        return throwError(() => error);
      })
    );
  }

  deleteClosure(siteId: string, closureId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${siteId}/closures/${closureId}`).pipe(
      tap(() => {
        this.store.deleteClosure(closureId);
      }),
      catchError(error => {
        this.store.setError(error.message);
        return throwError(() => error);
      })
    );
  }
}
