import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { TableModule } from 'primeng/table';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { Tag } from 'primeng/tag';
import { Toast } from 'primeng/toast';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { Skeleton } from 'primeng/skeleton';
import { Tooltip } from 'primeng/tooltip';
import { ConfirmationService, MessageService } from 'primeng/api';
import { SiteListDto, CreateSiteRequest } from '@core/models';
import { BreakpointService } from '@core/services';
import { SitesStore } from '../services/sites-store.service';
import { SiteFormDialogComponent } from '../components/site-form-dialog.component';

const STATUS_OPTIONS = [
  { label: 'All', value: null },
  { label: 'Active', value: true },
  { label: 'Inactive', value: false },
];

@Component({
  selector: 'app-sites-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    InputText,
    Select,
    ButtonModule,
    Tag,
    Toast,
    ConfirmDialog,
    Skeleton,
    Tooltip,
    SiteFormDialogComponent,
  ],
  providers: [ConfirmationService, MessageService],
  template: `
    <div class="page">
      <div class="page-header">
        <div class="page-header__row">
          <div>
            <h1 class="page-title">Sites Management</h1>
            <p class="page-subtitle">{{ store.totalCount() }} site(s)</p>
          </div>
          <p-button label="New Site" icon="pi pi-plus" (click)="openCreate()" />
        </div>

        <div class="filters">
          <input
            pInputText
            placeholder="Search sites..."
            [ngModel]="searchTerm"
            (ngModelChange)="onSearch($event)"
            class="search-input"
          />
          <p-select
            [options]="statusOptions"
            optionLabel="label"
            optionValue="value"
            [ngModel]="statusFilter"
            (ngModelChange)="onStatusFilter($event)"
            placeholder="Status"
            styleClass="status-filter"
          />
        </div>
      </div>

      <!-- Desktop table -->
      @if (bp.isAtLeast('md')) {
        <p-table
          [value]="store.sites()"
          [lazy]="true"
          [paginator]="true"
          [rows]="store.queryParams().pageSize ?? 10"
          [totalRecords]="store.totalCount()"
          [loading]="store.loading()"
          (onLazyLoad)="onPageChange($event)"
          [rowHover]="true"
          styleClass="p-datatable-sm"
        >
          <ng-template #header>
            <tr>
              <th>Name</th>
              <th>City</th>
              <th>Country</th>
              <th style="width:80px">Courts</th>
              <th style="width:90px">Status</th>
              <th style="width:140px">Actions</th>
            </tr>
          </ng-template>
          <ng-template #body let-site>
            <tr class="cursor-pointer" (click)="goToSite(site)">
              <td class="font-medium">{{ site.name }}</td>
              <td>{{ site.city }}</td>
              <td>{{ site.country }}</td>
              <td>{{ site.courtCount }}</td>
              <td>
                <p-tag
                  [value]="site.isActive ? 'Active' : 'Inactive'"
                  [severity]="site.isActive ? 'success' : 'danger'"
                />
              </td>
              <td (click)="$event.stopPropagation()">
                <div class="action-btns">
                  <p-button icon="pi pi-pencil" [text]="true" size="small" (click)="openEdit(site)" pTooltip="Edit" />
                  <p-button
                    [icon]="site.isActive ? 'pi pi-ban' : 'pi pi-check-circle'"
                    [text]="true"
                    size="small"
                    (click)="toggleActive(site)"
                    [pTooltip]="site.isActive ? 'Deactivate' : 'Activate'"
                  />
                  <p-button
                    icon="pi pi-trash"
                    [text]="true"
                    severity="danger"
                    size="small"
                    (click)="confirmDelete(site)"
                    pTooltip="Delete"
                  />
                </div>
              </td>
            </tr>
          </ng-template>
          <ng-template #emptymessage>
            <tr>
              <td colspan="6" class="text-center text-muted py-5">No sites found</td>
            </tr>
          </ng-template>
        </p-table>
      } @else {
        <!-- Mobile card layout -->
        @if (store.loading()) {
          @for (i of [1, 2, 3]; track i) {
            <div class="card mb-3">
              <p-skeleton height="1.2rem" width="60%" styleClass="mb-2" />
              <p-skeleton height="0.9rem" width="40%" />
            </div>
          }
        } @else if (store.sites().length === 0) {
          <div class="empty-state card">
            <p class="text-muted">No sites found</p>
          </div>
        } @else {
          <div class="cards-list">
            @for (site of store.sites(); track site.siteId) {
              <div class="card card--interactive site-mobile-card" (click)="goToSite(site)">
                <div class="site-mobile-card__top">
                  <span class="font-medium">{{ site.name }}</span>
                  <p-tag
                    [value]="site.isActive ? 'Active' : 'Inactive'"
                    [severity]="site.isActive ? 'success' : 'danger'"
                  />
                </div>
                <div class="text-sm text-muted">
                  {{ site.city }}, {{ site.country }} &middot; {{ site.courtCount }} court(s)
                </div>
                <div class="site-mobile-card__actions" (click)="$event.stopPropagation()">
                  <p-button icon="pi pi-pencil" [text]="true" size="small" (click)="openEdit(site)" />
                  <p-button
                    [icon]="site.isActive ? 'pi pi-ban' : 'pi pi-check-circle'"
                    [text]="true"
                    size="small"
                    (click)="toggleActive(site)"
                  />
                  <p-button icon="pi pi-trash" [text]="true" severity="danger" size="small" (click)="confirmDelete(site)" />
                </div>
              </div>
            }
          </div>

          <!-- Mobile pagination -->
          @if (store.totalPages() > 1) {
            <div class="mobile-pagination">
              <p-button
                label="Previous"
                icon="pi pi-chevron-left"
                [disabled]="(store.queryParams().page ?? 1) <= 1"
                severity="secondary"
                size="small"
                (click)="goToPage((store.queryParams().page ?? 1) - 1)"
              />
              <span class="text-sm text-muted">
                {{ store.queryParams().page ?? 1 }} / {{ store.totalPages() }}
              </span>
              <p-button
                label="Next"
                icon="pi pi-chevron-right"
                iconPos="right"
                [disabled]="(store.queryParams().page ?? 1) >= store.totalPages()"
                severity="secondary"
                size="small"
                (click)="goToPage((store.queryParams().page ?? 1) + 1)"
              />
            </div>
          }
        }
      }
    </div>

    <app-site-form-dialog
      [visible]="dialogVisible()"
      [site]="editingSite()"
      (visibleChange)="dialogVisible.set($event)"
      (saved)="onSave($event)"
    />

    <p-toast />
    <p-confirmdialog />
  `,
  styles: [
    `
      .page-header__row {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        gap: 1rem;
        flex-wrap: wrap;
      }
      .filters {
        display: flex;
        gap: 0.75rem;
        margin-top: 1rem;
        flex-wrap: wrap;
      }
      .search-input {
        flex: 1;
        min-width: 200px;
      }
      :host ::ng-deep .status-filter {
        min-width: 130px;
      }
      .action-btns {
        display: flex;
        gap: 0.25rem;
      }
      .cursor-pointer {
        cursor: pointer;
      }
      .cards-list {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
      }
      .site-mobile-card__top {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 0.25rem;
      }
      .site-mobile-card__actions {
        display: flex;
        gap: 0.25rem;
        margin-top: 0.5rem;
        justify-content: flex-end;
      }
      .mobile-pagination {
        display: flex;
        justify-content: center;
        align-items: center;
        gap: 1rem;
        margin-top: 1rem;
        padding: 0.5rem;
      }
      .empty-state {
        text-align: center;
        padding: 3rem 1rem;
      }
    `,
  ],
})
export class SitesListComponent implements OnInit, OnDestroy {
  readonly store = inject(SitesStore);
  readonly bp = inject(BreakpointService);
  private readonly router = inject(Router);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly messageService = inject(MessageService);

  readonly statusOptions = STATUS_OPTIONS;
  readonly dialogVisible = signal(false);
  readonly editingSite = signal<any>(null);

  searchTerm = '';
  statusFilter: boolean | null = null;

  private readonly search$ = new Subject<string>();
  private readonly destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.store.loadSites();

    this.search$
      .pipe(debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((term) => {
        this.store.updateQueryParams({ searchTerm: term || undefined, page: 1 });
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearch(term: string): void {
    this.searchTerm = term;
    this.search$.next(term);
  }

  onStatusFilter(val: boolean | null): void {
    this.statusFilter = val;
    this.store.updateQueryParams({ isActive: val, page: 1 });
  }

  onPageChange(event: any): void {
    const page = Math.floor(event.first / event.rows) + 1;
    this.store.updateQueryParams({ page, pageSize: event.rows });
  }

  goToPage(page: number): void {
    this.store.updateQueryParams({ page });
  }

  goToSite(site: SiteListDto): void {
    this.router.navigate(['/admin/sites', site.siteId]);
  }

  openCreate(): void {
    this.editingSite.set(null);
    this.dialogVisible.set(true);
  }

  openEdit(site: SiteListDto): void {
    this.editingSite.set(site);
    this.dialogVisible.set(true);
  }

  onSave(req: CreateSiteRequest): void {
    const editing = this.editingSite();
    const onSuccess = () => {
      this.dialogVisible.set(false);
      this.messageService.add({
        severity: 'success',
        summary: editing?.siteId ? 'Site updated' : 'Site created',
      });
    };
    const onError = () =>
      this.messageService.add({ severity: 'error', summary: 'Operation failed' });

    if (editing?.siteId) {
      this.store.updateSite(editing.siteId, req).subscribe({ next: onSuccess, error: onError });
    } else {
      this.store.createSite(req).subscribe({ next: onSuccess, error: onError });
    }
  }

  toggleActive(site: SiteListDto): void {
    this.store.toggleSiteActive(site.siteId, site.isActive).subscribe({
      next: () =>
        this.messageService.add({
          severity: 'success',
          summary: site.isActive ? 'Site deactivated' : 'Site activated',
        }),
      error: () =>
        this.messageService.add({ severity: 'error', summary: 'Operation failed' }),
    });
  }

  confirmDelete(site: SiteListDto): void {
    this.confirmationService.confirm({
      message: `Delete site "${site.name}"? This action cannot be undone.`,
      header: 'Confirm Deletion',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.store.deleteSite(site.siteId).subscribe({
          next: () =>
            this.messageService.add({ severity: 'success', summary: 'Site deleted' }),
          error: () =>
            this.messageService.add({ severity: 'error', summary: 'Could not delete site' }),
        });
      },
    });
  }
}
