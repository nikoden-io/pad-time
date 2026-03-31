// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import {ChangeDetectionStrategy, Component, computed, inject, OnInit, signal} from '@angular/core';
import {Router} from '@angular/router';
import {ConfirmDialog} from 'primeng/confirmdialog';
import {Toast} from 'primeng/toast';
import {ConfirmationService, MessageService} from 'primeng/api';
import {TranslocoDirective, TranslocoService, provideTranslocoScope} from '@jsverse/transloco';
import {Observable} from 'rxjs';
import {CreateSiteRequest, Site, SiteDetail} from '@core/models';
import {SitesRepository} from '@features/admin/data/stites.repository';
import {SitesStore} from '@features/admin/domain/store/sites.store';
import {
  SiteDetailModalComponent
} from '@features/admin/presentation/components/site-detail-modal/site-detail-modal.component';
import {
  SiteFormDialogComponent
} from '@features/admin/presentation/components/site-form-dialog/site-form-dialog.component';
import {PageShellComponent} from '@shared/components/page-shell/page-shell.component';

@Component({
  selector: 'app-sites-list',
  standalone: true,
  imports: [
    ConfirmDialog, Toast,
    SiteDetailModalComponent, SiteFormDialogComponent,
    TranslocoDirective, PageShellComponent,
  ],
  providers: [ConfirmationService, MessageService, provideTranslocoScope('sites')],
  templateUrl: './sites-list.component.html',
  styleUrl: './sites-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SitesListComponent implements OnInit {
  readonly showDetailModal = signal(false);
  readonly showSiteDialog = signal(false);
  readonly siteToEdit = signal<Site | null>(null);
  readonly selectedSiteForDetail = signal<SiteDetail | null>(null);
  readonly sitesLabel = computed(() => {
    const total = this.pagination().totalCount;
    const active = this.sites().filter(s => s.isActive).length;
    return `${total} site${total > 1 ? 's' : ''} · ${active} actif${active > 1 ? 's' : ''}`;
  });
  private readonly repository = inject(SitesRepository);
  private readonly store = inject(SitesStore);
  readonly sites = this.store.sites;
  readonly loading = this.store.isLoadingSites;
  readonly pagination = this.store.pagination;
  private readonly transloco = inject(TranslocoService);
  private readonly router = inject(Router);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toast = inject(MessageService);

  ngOnInit(): void {
    this.loadSites();
  }

  goToDashboard(): void {
    this.router.navigate(['/admin']);
  }

  loadSites(page = 1, pageSize = 10): void {
    this.repository.getSites(page, pageSize).subscribe({
      error: () => this.showToast('error', 'errors.generic', 'sites.messages.loadError'),
    });
  }

  onCreateSite(): void {
    this.siteToEdit.set(null);
    this.showSiteDialog.set(true);
  }

  onViewSite(siteId: string): void {
    this.repository.getSiteById(siteId).subscribe({
      next: () => {
        this.selectedSiteForDetail.set(this.store.selectedSite());
        this.showDetailModal.set(true);
      },
      error: () => this.showToast('error', 'errors.generic', 'sites.messages.loadDetailError'),
    });
  }

  onEditSite(site: Site | SiteDetail): void {
    this.siteToEdit.set(site as Site);
    this.showSiteDialog.set(true);
  }

  onSiteDialogSubmit(request: CreateSiteRequest): void {
    const site = this.siteToEdit();
    const isUpdate = site !== null;
    const successKey = isUpdate ? 'sites.messages.updateSuccess' : 'sites.messages.createSuccess';
    const errorKey = isUpdate ? 'sites.messages.updateError' : 'sites.messages.createError';

    const op$ = (site
      ? this.repository.updateSite(site.siteId, request)
      : this.repository.createSite(request)) as Observable<any>;

    op$.subscribe({
      next: () => {
        this.showToast('success', 'common.success', successKey);
        this.showSiteDialog.set(false);
        this.loadSites();
      },
      error: () => this.showToast('error', 'errors.generic', errorKey),
    });
  }

  onToggleActive(site: Site): void {
    const action = site.isActive ? 'deactivate' : 'activate';
    this.confirmation.confirm({
      message: this.transloco.translate('sites.messages.activateConfirm', {action, name: site.name}),
      header: this.transloco.translate('common.confirm'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.repository.toggleSiteActive(site.siteId, !site.isActive).subscribe({
          next: () => this.showToast('success', 'common.success', 'sites.messages.activateSuccess'),
          error: () => this.showToast('error', 'errors.generic', 'sites.messages.activateError'),
        });
      },
    });
  }

  onDeleteSite(site: Site): void {
    this.confirmation.confirm({
      message: this.transloco.translate('sites.messages.deleteConfirm', {name: site.name}),
      header: this.transloco.translate('confirmation.deleteTitle'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.repository.deleteSite(site.siteId).subscribe({
          next: () => {
            this.showToast('success', 'common.success', 'sites.messages.deleteSuccess');
            this.loadSites();
          },
          error: () => this.showToast('error', 'errors.generic', 'sites.messages.deleteError'),
        });
      },
    });
  }

  private showToast(severity: 'success' | 'error', summaryKey: string, detailKey: string): void {
    this.toast.add({
      severity,
      summary: this.transloco.translate(summaryKey),
      detail: this.transloco.translate(detailKey),
    });
  }
}