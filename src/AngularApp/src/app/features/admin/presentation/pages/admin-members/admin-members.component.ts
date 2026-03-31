// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import {ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal} from '@angular/core';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {RouterLink} from '@angular/router';
import {SlicePipe} from '@angular/common';
import {TranslocoDirective, TranslocoService} from '@jsverse/transloco';
import {ApiService} from '@core/services';
import {AdminMember, AdminMemberDetail, PaginatedResponse} from '@core/models';
import {PageShellComponent} from '@shared/components/page-shell/page-shell.component';

@Component({
  selector: 'app-admin-members',
  standalone: true,
  imports: [RouterLink, SlicePipe, TranslocoDirective, PageShellComponent],
  templateUrl: './admin-members.component.html',
  styleUrl: './admin-members.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminMembersComponent implements OnInit {
  // ── State ──────────────────────────────────────────
  readonly members = signal<AdminMember[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);
  readonly page = signal(1);
  readonly pageSize = signal(50);

  readonly filterCategory = signal<string>('');
  readonly filterStatus = signal<string>('');
  readonly filterSearch = signal('');

  readonly selectedMember = signal<AdminMemberDetail | null>(null);
  readonly detailLoading = signal(false);
  readonly showDetail = signal(false);

  // ── Computed stats ─────────────────────────────────
  readonly totalMembers = computed(() => this.totalCount());
  readonly activeMembers = computed(() => this.members().filter(m => m.isActive).length);
  readonly globalMembers = computed(() => this.members().filter(m => m.category === 'Global').length);
  readonly siteMembers = computed(() => this.members().filter(m => m.category === 'Site').length);
  readonly freeMembers = computed(() => this.members().filter(m => m.category === 'Free').length);
  readonly membersWithDebt = computed(() => this.members().filter(m => m.debtAmountCents > 0).length);
  readonly totalDebtEuros = computed(() =>
    (this.members().reduce((s, m) => s + m.debtAmountCents, 0) / 100).toFixed(2)
  );

  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly transloco = inject(TranslocoService);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const cat = this.filterCategory();
    const status = this.filterStatus();

    this.api.getAdminMembers({
      page: this.page(),
      pageSize: this.pageSize(),
      category: cat || undefined,
      isActive: status === '' ? undefined : status === 'active',
      search: this.filterSearch() || undefined,
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        this.members.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onCategoryChange(value: string): void {
    this.filterCategory.set(value);
    this.page.set(1);
    this.load();
  }

  onStatusChange(value: string): void {
    this.filterStatus.set(value);
    this.page.set(1);
    this.load();
  }

  onSearchInput(value: string): void {
    this.filterSearch.set(value);
    this.page.set(1);
    this.load();
  }

  openDetail(member: AdminMember): void {
    this.showDetail.set(true);
    this.detailLoading.set(true);
    this.selectedMember.set(null);

    this.api.getAdminMemberDetail(member.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (detail) => {
          this.selectedMember.set(detail);
          this.detailLoading.set(false);
        },
        error: () => this.detailLoading.set(false),
      });
  }

  closeDetail(): void {
    this.showDetail.set(false);
    this.selectedMember.set(null);
  }

  toggleStatus(member: AdminMember): void {
    const action$ = member.isActive
      ? this.api.deactivateMember(member.id)
      : this.api.activateMember(member.id);

    action$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => this.load(),
    });
  }

  categoryLabel(cat: string): string {
    return this.transloco.translate(`admin.members.categories.${cat.toLowerCase()}`);
  }

  categoryIcon(cat: string): string {
    switch (cat) {
      case 'Global': return '🌍';
      case 'Site': return '📍';
      case 'Free': return '🎾';
      default: return '👤';
    }
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('fr-BE', {day: '2-digit', month: 'short', year: 'numeric'});
  }

  formatDateTime(iso: string): string {
    return new Date(iso).toLocaleString('fr-BE', {day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit'});
  }

  formatEuros(cents: number): string {
    return (cents / 100).toLocaleString('fr-BE', {style: 'currency', currency: 'EUR', minimumFractionDigits: 2});
  }

  matchStatusLabel(status: string): string {
    return this.transloco.translate(`admin.members.matchStatus.${status}`);
  }
}