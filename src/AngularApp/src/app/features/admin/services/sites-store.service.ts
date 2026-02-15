import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap, finalize } from 'rxjs';
import { SitesApiService } from '@core/services';
import {
  SiteListDto,
  SiteDetailDto,
  SitesQueryParams,
  CreateSiteRequest,
  UpdateSiteRequest,
  CreateCourtRequest,
  UpdateCourtRequest,
  CreateScheduleRequest,
  UpdateScheduleRequest,
  CreateClosureRequest,
} from '@core/models';

@Injectable()
export class SitesStore {
  private readonly api = inject(SitesApiService);

  // ── List state ──
  readonly sites = signal<SiteListDto[]>([]);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);
  readonly loading = signal(false);
  readonly queryParams = signal<SitesQueryParams>({ page: 1, pageSize: 10 });

  // ── Detail state ──
  readonly currentSite = signal<SiteDetailDto | null>(null);
  readonly detailLoading = signal(false);

  // ── Sites ──

  loadSites(): void {
    this.loading.set(true);
    this.api
      .getSites(this.queryParams())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          this.sites.set(res.data);
          this.totalCount.set(res.totalCount);
          this.totalPages.set(res.totalPages);
        },
      });
  }

  updateQueryParams(params: Partial<SitesQueryParams>): void {
    this.queryParams.update((prev) => ({ ...prev, ...params }));
    this.loadSites();
  }

  loadSite(siteId: string): void {
    this.detailLoading.set(true);
    this.api
      .getSite(siteId)
      .pipe(finalize(() => this.detailLoading.set(false)))
      .subscribe({ next: (site) => this.currentSite.set(site) });
  }

  createSite(req: CreateSiteRequest): Observable<{ siteId: string }> {
    return this.api.createSite(req).pipe(tap(() => this.loadSites()));
  }

  updateSite(siteId: string, req: UpdateSiteRequest): Observable<void> {
    return this.api.updateSite(siteId, req).pipe(
      tap(() => {
        this.loadSites();
        this.loadSite(siteId);
      }),
    );
  }

  deleteSite(siteId: string): Observable<void> {
    return this.api.deleteSite(siteId).pipe(tap(() => this.loadSites()));
  }

  toggleSiteActive(siteId: string, currentlyActive: boolean): Observable<void> {
    const action$ = currentlyActive
      ? this.api.deactivateSite(siteId)
      : this.api.activateSite(siteId);
    return action$.pipe(
      tap(() => {
        this.loadSites();
        if (this.currentSite()?.siteId === siteId) this.loadSite(siteId);
      }),
    );
  }

  // ── Courts ──

  createCourt(siteId: string, req: CreateCourtRequest): Observable<{ courtId: string }> {
    return this.api.createCourt(siteId, req).pipe(tap(() => this.loadSite(siteId)));
  }

  updateCourt(siteId: string, courtId: string, req: UpdateCourtRequest): Observable<void> {
    return this.api.updateCourt(siteId, courtId, req).pipe(tap(() => this.loadSite(siteId)));
  }

  deleteCourt(siteId: string, courtId: string): Observable<void> {
    return this.api.deleteCourt(siteId, courtId).pipe(tap(() => this.loadSite(siteId)));
  }

  // ── Schedules ──

  createSchedule(siteId: string, req: CreateScheduleRequest): Observable<{ scheduleId: string }> {
    return this.api.createSchedule(siteId, req).pipe(tap(() => this.loadSite(siteId)));
  }

  updateSchedule(
    siteId: string,
    scheduleId: string,
    req: UpdateScheduleRequest,
  ): Observable<void> {
    return this.api
      .updateSchedule(siteId, scheduleId, req)
      .pipe(tap(() => this.loadSite(siteId)));
  }

  deleteSchedule(siteId: string, scheduleId: string): Observable<void> {
    return this.api.deleteSchedule(siteId, scheduleId).pipe(tap(() => this.loadSite(siteId)));
  }

  // ── Closures ──

  createClosure(siteId: string, req: CreateClosureRequest): Observable<{ closureId: string }> {
    return this.api.createClosure(siteId, req).pipe(tap(() => this.loadSite(siteId)));
  }

  deleteClosure(siteId: string, closureId: string): Observable<void> {
    return this.api.deleteClosure(siteId, closureId).pipe(tap(() => this.loadSite(siteId)));
  }
}
