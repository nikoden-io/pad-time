import {Injectable, signal, computed} from '@angular/core';
import {Site, SiteDetail, Court, Schedule, Closure, PagedResult} from '@core/models';

export interface SitesState {
  sites: Site[];
  selectedSite: SiteDetail | null;

  courts: Court[];
  schedules: Schedule[];
  closures: Closure[];

  loading: {
    sites: boolean;
    siteDetail: boolean;
    courts: boolean;
    schedules: boolean;
    closures: boolean;
  };

  error: string | null;

  pagination: {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
}

const initialState: SitesState = {
  sites: [],
  selectedSite: null,
  courts: [],
  schedules: [],
  closures: [],
  loading: {
    sites: false,
    siteDetail: false,
    courts: false,
    schedules: false,
    closures: false,
  },
  error: null,
  pagination: {
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
  },
};

@Injectable({providedIn: 'root'})
export class SitesStore {
  private state = signal<SitesState>(initialState);
  sites = computed(() => this.state().sites);
  activeSites = computed(() =>
    this.sites().filter(site => site.isActive)
  );
  selectedSite = computed(() => this.state().selectedSite);
  courts = computed(() => this.state().courts);
  activeCourts = computed(() =>
    this.courts().filter(court => court.isActive)
  );
  schedules = computed(() => this.state().schedules);
  closures = computed(() => this.state().closures);

  loading = computed(() => this.state().loading);
  isLoadingSites = computed(() => this.state().loading.sites);
  isLoadingSiteDetail = computed(() => this.state().loading.siteDetail);

  error = computed(() => this.state().error);

  pagination = computed(() => this.state().pagination);
  hasNextPage = computed(() =>
    this.state().pagination.page < this.state().pagination.totalPages
  );
  hasPreviousPage = computed(() =>
    this.state().pagination.page > 1
  );

  setSites(pagedResult: PagedResult<Site>) {
    this.state.update(state => ({
      ...state,
      sites: pagedResult.items,
      pagination: {
        page: pagedResult.page,
        pageSize: pagedResult.pageSize,
        totalCount: pagedResult.totalCount,
        totalPages: pagedResult.totalPages,
      },
      loading: {...state.loading, sites: false},
      error: null,
    }));
  }

  setSelectedSite(site: SiteDetail | null) {
    this.state.update(state => ({
      ...state,
      selectedSite: site,
      courts: site ? state.courts : [],
      schedules: site ? state.schedules : [],
      closures: site ? state.closures : [],
      loading: {...state.loading, siteDetail: false},
      error: null,
    }));
  }

  addSite(site: Site) {
    this.state.update(state => ({
      ...state,
      sites: [site, ...state.sites],
      pagination: {
        ...state.pagination,
        totalCount: state.pagination.totalCount + 1,
      },
    }));
  }

  updateSite(updatedSite: Site) {
    this.state.update(state => ({
      ...state,
      sites: state.sites.map(site =>
        site.siteId === updatedSite.siteId ? updatedSite : site
      ),
      selectedSite: state.selectedSite?.siteId === updatedSite.siteId
        ? {
          ...state.selectedSite,
          name: updatedSite.name,
          streetNumber: updatedSite.streetNumber,
          street: updatedSite.street,
          postcode: updatedSite.postcode,
          city: updatedSite.city,
          country: updatedSite.country,
          timezone: updatedSite.timezone,
          isActive: updatedSite.isActive,
        }
        : state.selectedSite,
    }));
  }

  deleteSite(siteId: string) {
    this.state.update(state => ({
      ...state,
      sites: state.sites.filter(site => site.siteId !== siteId),
      selectedSite: state.selectedSite?.siteId === siteId ? null : state.selectedSite,
      pagination: {
        ...state.pagination,
        totalCount: state.pagination.totalCount - 1,
      },
    }));
  }

  toggleSiteActive(siteId: string, isActive: boolean) {
    this.state.update(state => ({
      ...state,
      sites: state.sites.map(site =>
        site.siteId === siteId ? {...site, isActive} : site
      ),
    }));
  }

  setCourts(courts: Court[]) {
    this.state.update(state => ({
      ...state,
      courts,
      loading: {...state.loading, courts: false},
      error: null,
    }));
  }

  addCourt(court: Court) {
    this.state.update(state => ({
      ...state,
      courts: [...state.courts, court],
    }));
  }

  updateCourt(updatedCourt: Court) {
    this.state.update(state => ({
      ...state,
      courts: state.courts.map(court =>
        court.courtId === updatedCourt.courtId ? updatedCourt : court
      ),
    }));
  }

  deleteCourt(courtId: string) {
    this.state.update(state => ({
      ...state,
      courts: state.courts.filter(court => court.courtId !== courtId),
    }));
  }

  setSchedules(schedules: Schedule[]) {
    this.state.update(state => ({
      ...state,
      schedules,
      loading: {...state.loading, schedules: false},
      error: null,
    }));
  }

  addSchedule(schedule: Schedule) {
    this.state.update(state => ({
      ...state,
      schedules: [...state.schedules, schedule],
    }));
  }

  updateSchedule(updatedSchedule: Schedule) {
    this.state.update(state => ({
      ...state,
      schedules: state.schedules.map(schedule =>
        schedule.scheduleId === updatedSchedule.scheduleId ? updatedSchedule : schedule
      ),
    }));
  }

  deleteSchedule(scheduleId: string) {
    this.state.update(state => ({
      ...state,
      schedules: state.schedules.filter(schedule => schedule.scheduleId !== scheduleId),
    }));
  }

  setClosures(closures: Closure[]) {
    this.state.update(state => ({
      ...state,
      closures,
      loading: {...state.loading, closures: false},
      error: null,
    }));
  }

  addClosure(closure: Closure) {
    this.state.update(state => ({
      ...state,
      closures: [...state.closures, closure],
    }));
  }

  deleteClosure(closureId: string) {
    this.state.update(state => ({
      ...state,
      closures: state.closures.filter(closure => closure.closureId !== closureId),
    }));
  }

  setLoading(entity: keyof SitesState['loading'], loading: boolean) {
    this.state.update(state => ({
      ...state,
      loading: {...state.loading, [entity]: loading},
    }));
  }

  setError(error: string | null) {
    this.state.update(state => ({
      ...state,
      error,
      loading: {
        sites: false,
        siteDetail: false,
        courts: false,
        schedules: false,
        closures: false,
      },
    }));
  }

  reset() {
    this.state.set(initialState);
  }

  resetRelatedEntities() {
    this.state.update(state => ({
      ...state,
      courts: [],
      schedules: [],
      closures: [],
    }));
  }
}
