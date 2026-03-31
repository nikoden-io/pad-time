// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import { computed, DestroyRef, inject, Injectable, signal } from '@angular/core';

/**
 * Mobile-first breakpoints (min-width).
 * Usage in SCSS: @media (min-width: 640px) { ... }
 */
export const Breakpoints = {
  xs: 0,
  sm: 640,
  md: 768,
  lg: 1024,
  xl: 1280,
  '2xl': 1536,
} as const;

export type BreakpointKey = keyof typeof Breakpoints;

@Injectable({ providedIn: 'root' })
export class BreakpointService {
  private readonly destroyRef = inject(DestroyRef);

  // Individual match signals — true when viewport >= breakpoint
  private readonly smMatch = signal(false);
  private readonly mdMatch = signal(false);
  private readonly lgMatch = signal(false);
  private readonly xlMatch = signal(false);
  private readonly xxlMatch = signal(false);

  /** Resolved current breakpoint key */
  readonly current = computed<BreakpointKey>(() => {
    if (this.xxlMatch()) return '2xl';
    if (this.xlMatch()) return 'xl';
    if (this.lgMatch()) return 'lg';
    if (this.mdMatch()) return 'md';
    if (this.smMatch()) return 'sm';
    return 'xs';
  });

  /** Convenience booleans */
  readonly isMobile = computed(() => !this.mdMatch());
  readonly isTablet = computed(() => this.mdMatch() && !this.lgMatch());
  readonly isDesktop = computed(() => this.lgMatch());

  /** True when viewport >= given breakpoint */
  readonly sm = this.smMatch.asReadonly();
  readonly md = this.mdMatch.asReadonly();
  readonly lg = this.lgMatch.asReadonly();
  readonly xl = this.xlMatch.asReadonly();
  readonly xxl = this.xxlMatch.asReadonly();

  /** Viewport width in px (updated on resize) */
  readonly width = signal(window.innerWidth);

  private readonly queries: { mql: MediaQueryList; handler: (e: MediaQueryListEvent) => void }[] = [];

  constructor() {
    this.registerQuery(Breakpoints.sm, this.smMatch);
    this.registerQuery(Breakpoints.md, this.mdMatch);
    this.registerQuery(Breakpoints.lg, this.lgMatch);
    this.registerQuery(Breakpoints.xl, this.xlMatch);
    this.registerQuery(Breakpoints['2xl'], this.xxlMatch);

    const onResize = () => this.width.set(window.innerWidth);
    window.addEventListener('resize', onResize, { passive: true });

    this.destroyRef.onDestroy(() => {
      window.removeEventListener('resize', onResize);
      this.queries.forEach(({ mql, handler }) => mql.removeEventListener('change', handler));
    });
  }

  /** Check if viewport is at least the given breakpoint */
  isAtLeast(bp: BreakpointKey): boolean {
    return this.width() >= Breakpoints[bp];
  }

  private registerQuery(minWidth: number, target: ReturnType<typeof signal<boolean>>): void {
    const mql = window.matchMedia(`(min-width: ${minWidth}px)`);
    target.set(mql.matches);
    const handler = (e: MediaQueryListEvent) => target.set(e.matches);
    mql.addEventListener('change', handler);
    this.queries.push({ mql, handler });
  }
}