import {Component, inject, OnInit, signal} from '@angular/core';
import {CommonModule} from '@angular/common';
import {Router} from '@angular/router';
import {TableModule} from 'primeng/table';
import {ButtonModule} from 'primeng/button';
import {Tag} from 'primeng/tag';
import {ConfirmDialog} from 'primeng/confirmdialog';
import {Toast} from 'primeng/toast';
import {ConfirmationService, MessageService} from 'primeng/api';
import {TranslocoDirective, TranslocoService, provideTranslocoScope} from '@jsverse/transloco';
import {CreateSiteRequest, Site, SiteDetail} from '@core/models';
import {SitesRepository} from '@features/admin/data/stites.repository';
import {SitesStore} from '@features/admin/domain/store/sites.store';
import {
  SiteDetailModalComponent
} from '@features/admin/presentation/components/site-detail-modal/site-detail-modal.component';
import {
  SiteFormDialogComponent
} from '@features/admin/presentation/components/site-form-dialog/site-form-dialog.component';
import {Observable} from 'rxjs';

@Component({
  selector: 'app-sites-list',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    Tag,
    ConfirmDialog,
    Toast,
    SiteDetailModalComponent,
    SiteFormDialogComponent,
    TranslocoDirective,
  ],
  providers: [ConfirmationService, MessageService, provideTranslocoScope('sites')],
  templateUrl: './sites-list.component.html',
  styleUrl: './sites-list.component.scss',
})
export class SitesListComponent implements OnInit {
  showDetailModal = signal(false);
  showSiteDialog = signal(false);
  siteToEdit = signal<Site | null>(null);
  selectedSiteForDetail = signal<SiteDetail | null>(null);
  private readonly repository = inject(SitesRepository);
  private readonly store = inject(SitesStore);
  sites = this.store.sites;
  loading = this.store.isLoadingSites;
  pagination = this.store.pagination;
  private readonly translocoService = inject(TranslocoService);
  private readonly router = inject(Router);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly messageService = inject(MessageService);

  ngOnInit(): void {
    this.loadSites();
  }

  loadSites(page: number = 1, pageSize: number = 10): void {
    this.repository.getSites(page, pageSize).subscribe({
      error: () => {
        this.showToast('error', 'errors.generic', 'sites.messages.loadError');
      },
    });
  }

  onPageChange(event: any): void {
    const page = (event.first / event.rows) + 1;
    this.loadSites(page, event.rows);
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
      error: () => {
        this.showToast('error', 'errors.generic', 'sites.messages.loadDetailError');
      },
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

    const operation$ = (site
      ? this.repository.updateSite(site.siteId, request)
      : this.repository.createSite(request)) as Observable<any>;

    operation$.subscribe({
      next: () => {
        this.showToast('success', 'common.success', successKey);
        this.showSiteDialog.set(false);
        this.loadSites();
      },
      error: () => {
        this.showToast('error', 'errors.generic', errorKey);
      },
    });
  }

  onToggleActive(site: Site): void {
    const action = site.isActive ? 'deactivate' : 'activate';

    this.confirmationService.confirm({
      message: this.translocoService.translate('sites.messages.activateConfirm', {action, name: site.name}),
      header: this.translocoService.translate('common.confirm'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.repository.toggleSiteActive(site.siteId, !site.isActive).subscribe({
          next: () => {
            this.showToast('success', 'common.success', 'sites.messages.activateSuccess');
          },
          error: () => {
            this.showToast('error', 'errors.generic', 'sites.messages.activateError');
          },
        });
      },
    });
  }

  onDeleteSite(site: Site): void {
    this.confirmationService.confirm({
      message: this.translocoService.translate('sites.messages.deleteConfirm', {name: site.name}),
      header: this.translocoService.translate('confirmation.deleteTitle'),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.repository.deleteSite(site.siteId).subscribe({
          next: () => {
            this.showToast('success', 'common.success', 'sites.messages.deleteSuccess');
            this.loadSites();
          },
          error: () => {
            this.showToast('error', 'errors.generic', 'sites.messages.deleteError');
          },
        });
      },
    });
  }

  private showToast(severity: 'success' | 'error', summaryKey: string, detailKey: string): void {
    this.messageService.add({
      severity,
      summary: this.translocoService.translate(summaryKey),
      detail: this.translocoService.translate(detailKey),
    });
  }
}
