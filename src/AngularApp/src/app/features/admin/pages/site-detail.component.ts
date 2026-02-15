import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { Tag } from 'primeng/tag';
import { Tabs, TabList, Tab, TabPanels, TabPanel } from 'primeng/tabs';
import { Toast } from 'primeng/toast';
import { Skeleton } from 'primeng/skeleton';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { CreateSiteRequest } from '@core/models';
import { SitesStore } from '../services/sites-store.service';
import { SiteFormDialogComponent } from '../components/site-form-dialog.component';
import { CourtsTabComponent } from '../components/courts-tab.component';
import { SchedulesTabComponent } from '../components/schedules-tab.component';

@Component({
  selector: 'app-site-detail',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    ButtonModule,
    Tag,
    Tabs,
    TabList,
    Tab,
    TabPanels,
    TabPanel,
    Toast,
    Skeleton,
    ConfirmDialog,
    SiteFormDialogComponent,
    CourtsTabComponent,
    SchedulesTabComponent,
  ],
  providers: [ConfirmationService, MessageService],
  template: `
    <div class="page">
      @if (store.detailLoading()) {
        <div class="skeleton-header">
          <p-skeleton height="1.5rem" width="40%" styleClass="mb-2" />
          <p-skeleton height="1rem" width="60%" />
        </div>
      } @else if (site(); as s) {
        <!-- Header -->
        <div class="detail-header">
          <div class="detail-header__left">
            <p-button
              icon="pi pi-arrow-left"
              [text]="true"
              severity="secondary"
              (click)="goBack()"
              pTooltip="Back to sites"
            />
            <div>
              <div class="detail-header__title-row">
                <h1 class="page-title">{{ s.name }}</h1>
                <p-tag
                  [value]="s.isActive ? 'Active' : 'Inactive'"
                  [severity]="s.isActive ? 'success' : 'danger'"
                />
              </div>
              <p class="page-subtitle">
                {{ s.streetNumber }} {{ s.street }}, {{ s.postcode }} {{ s.city }}, {{ s.country }}
                &middot; {{ s.timezone }}
              </p>
            </div>
          </div>
          <div class="detail-header__actions">
            <p-button
              [label]="s.isActive ? 'Deactivate' : 'Activate'"
              [severity]="s.isActive ? 'warn' : 'success'"
              [outlined]="true"
              size="small"
              (click)="toggleActive()"
            />
            <p-button label="Edit" icon="pi pi-pencil" size="small" (click)="openEditDialog()" />
          </div>
        </div>

        <!-- Tabs -->
        <p-tabs [value]="activeTab()" (valueChange)="activeTab.set(+$event!)">
          <p-tablist>
            <p-tab [value]="0">Courts ({{ s.courts.length }})</p-tab>
            <p-tab [value]="1">Schedules ({{ s.schedules.length }})</p-tab>
            <p-tab [value]="2">Info</p-tab>
          </p-tablist>
          <p-tabpanels>
            <p-tabpanel [value]="0">
              <app-courts-tab [siteId]="s.siteId" [courts]="s.courts" />
            </p-tabpanel>
            <p-tabpanel [value]="1">
              <app-schedules-tab
                [siteId]="s.siteId"
                [schedules]="s.schedules"
                [closures]="s.closures"
                [courts]="s.courts"
              />
            </p-tabpanel>
            <p-tabpanel [value]="2">
              <div class="info-grid">
                <div class="info-item">
                  <span class="info-label">Created</span>
                  <span>{{ s.createdAtUtc | date: 'medium' }}</span>
                </div>
                @if (s.updatedAtUtc) {
                  <div class="info-item">
                    <span class="info-label">Last Updated</span>
                    <span>{{ s.updatedAtUtc | date: 'medium' }}</span>
                  </div>
                }
                <div class="info-item">
                  <span class="info-label">Timezone</span>
                  <span>{{ s.timezone }}</span>
                </div>
                <div class="info-item">
                  <span class="info-label">Address</span>
                  <span>{{ s.streetNumber }} {{ s.street }}, {{ s.postcode }} {{ s.city }}</span>
                </div>
                <div class="info-item">
                  <span class="info-label">Country</span>
                  <span>{{ s.country }}</span>
                </div>
                <div class="info-item">
                  <span class="info-label">Courts</span>
                  <span>{{ s.courts.length }}</span>
                </div>
                <div class="info-item">
                  <span class="info-label">Schedules</span>
                  <span>{{ s.schedules.length }}</span>
                </div>
                <div class="info-item">
                  <span class="info-label">Closures</span>
                  <span>{{ s.closures.length }}</span>
                </div>
              </div>
            </p-tabpanel>
          </p-tabpanels>
        </p-tabs>
      } @else {
        <div class="empty-state">
          <p class="text-muted">Site not found.</p>
          <p-button label="Back to sites" (click)="goBack()" />
        </div>
      }
    </div>

    <app-site-form-dialog
      [visible]="editDialogVisible()"
      [site]="site()"
      (visibleChange)="editDialogVisible.set($event)"
      (saved)="onSave($event)"
    />

    <p-toast />
    <p-confirmdialog />
  `,
  styles: [
    `
      .skeleton-header {
        padding: 1rem 0 2rem;
      }
      .detail-header {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        gap: 1rem;
        margin-bottom: 1.5rem;
        flex-wrap: wrap;
      }
      .detail-header__left {
        display: flex;
        align-items: flex-start;
        gap: 0.5rem;
      }
      .detail-header__title-row {
        display: flex;
        align-items: center;
        gap: 0.75rem;
      }
      .detail-header__actions {
        display: flex;
        gap: 0.5rem;
        flex-shrink: 0;
      }
      .info-grid {
        display: grid;
        grid-template-columns: 1fr;
        gap: 0.75rem;
        padding: 0.5rem 0;
      }
      @media (min-width: 640px) {
        .info-grid {
          grid-template-columns: 1fr 1fr;
        }
      }
      .info-item {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
      }
      .info-label {
        font-size: 0.75rem;
        font-weight: 600;
        text-transform: uppercase;
        color: var(--pt-text-muted);
      }
      .empty-state {
        text-align: center;
        padding: 3rem 1rem;
      }
    `,
  ],
})
export class SiteDetailComponent implements OnInit {
  readonly store = inject(SitesStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);

  readonly site = computed(() => this.store.currentSite());
  readonly activeTab = signal(0);
  readonly editDialogVisible = signal(false);

  ngOnInit(): void {
    const siteId = this.route.snapshot.params['siteId'];
    if (siteId) this.store.loadSite(siteId);
  }

  goBack(): void {
    this.router.navigate(['/admin/sites']);
  }

  openEditDialog(): void {
    this.editDialogVisible.set(true);
  }

  onSave(req: CreateSiteRequest): void {
    const s = this.site();
    if (!s) return;
    this.store.updateSite(s.siteId, req).subscribe({
      next: () => {
        this.editDialogVisible.set(false);
        this.messageService.add({ severity: 'success', summary: 'Site updated' });
      },
      error: () =>
        this.messageService.add({ severity: 'error', summary: 'Update failed' }),
    });
  }

  toggleActive(): void {
    const s = this.site();
    if (!s) return;
    this.store.toggleSiteActive(s.siteId, s.isActive).subscribe({
      next: () =>
        this.messageService.add({
          severity: 'success',
          summary: s.isActive ? 'Site deactivated' : 'Site activated',
        }),
      error: () =>
        this.messageService.add({ severity: 'error', summary: 'Operation failed' }),
    });
  }
}
