# Pad'Time — Angular Front-end

## Stack

- **Angular 21** — Standalone components, Signals
- **PrimeNG 21** — Aura theme with dark/light mode
- **SCSS** — CSS custom properties + mobile-first mixins
- **OIDC** — Authentication via `angular-auth-oidc-client`

---

## 1. Scalable component architecture

### Folder structure

```
src/app/
├── core/               # Singletons, initialized once
│   ├── auth/           # AuthService, auth.config
│   ├── guards/         # Route guards (auth, admin)
│   ├── interceptors/   # HTTP interceptors
│   ├── models/         # Shared interfaces/types
│   └── services/       # Global services (API, Layout, Breakpoint)
│
├── features/           # Business modules (lazy-loaded)
│   ├── admin/
│   ├── auth/
│   └── booking/
│
├── layout/             # Shell, Navbar (page structure)
│   └── components/
│
└── shared/             # Reusable components/directives/pipes
    ├── components/
    ├── directives/
    └── pipes/
```

### Layer rules

| Layer        | Can import                            | Provided via        |
|-------------|---------------------------------------|----------------------|
| `core/`     | Only `@angular/*`                     | `providedIn: 'root'` |
| `features/` | `core/`, `shared/`                    | Lazy-loaded routes   |
| `shared/`   | `core/` (services only)               | Direct import        |
| `layout/`   | `core/`, `shared/`                    | Direct import        |

### Creating a new feature module

```
src/app/features/my-feature/
├── my-feature.routes.ts         # Lazy-loaded routes
├── pages/                       # Smart components (inject services)
│   └── my-feature-page.component.ts
├── components/                  # Dumb components (inputs/outputs only)
│   └── my-widget/
│       ├── my-widget.component.ts
│       ├── my-widget.component.html
│       └── my-widget.component.scss
└── services/                    # Feature-specific services (optional)
```

**Routes (lazy-loaded)** — `my-feature.routes.ts`:

```typescript
import { Routes } from '@angular/router';

export const myFeatureRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/my-feature-page.component')
        .then(m => m.MyFeaturePageComponent),
  },
];
```

**Register in `app.routes.ts`**:

```typescript
{
  path: 'my-feature',
  loadChildren: () =>
    import('./features/my-feature/my-feature.routes')
      .then(m => m.myFeatureRoutes),
}
```

### Smart / Dumb component pattern

**Smart component** (page) — orchestrates, injects services:

```typescript
@Component({
  selector: 'app-my-feature-page',
  standalone: true,
  imports: [MyWidgetComponent],
  template: `
    <div class="page">
      <div class="page-header">
        <h1 class="page-title">My Feature</h1>
      </div>
      <app-my-widget [data]="items()" (selected)="onSelect($event)" />
    </div>
  `,
})
export class MyFeaturePageComponent {
  private readonly api = inject(ApiService);
  readonly items = signal<Item[]>([]);
}
```

**Dumb component** — receives `input()`, emits `output()`:

```typescript
@Component({
  selector: 'app-my-widget',
  standalone: true,
  template: `...`,
  styleUrl: './my-widget.component.scss',
})
export class MyWidgetComponent {
  readonly data = input.required<Item[]>();
  readonly selected = output<Item>();
}
```

### State management with Signals

```typescript
readonly items = signal<Item[]>([]);
readonly loading = signal(false);
readonly selectedId = signal<string | null>(null);

readonly selectedItem = computed(() =>
  this.items().find(i => i.id === this.selectedId())
);
readonly count = computed(() => this.items().length);
```

---

## 2. Theming and responsive system

### Dark / Light mode

Theming is managed by `LayoutService` (`core/services/layout-service.ts`).

**3 modes**: `light`, `dark`, `system` (follows OS preference).

```typescript
private readonly layout = inject(LayoutService);

// Read state
this.layout.isDark()           // resolved boolean
this.layout.themePreference()  // 'light' | 'dark' | 'system'

// Change theme
this.layout.setTheme('dark');
this.layout.toggleTheme();     // cycle: light → dark → system
```

Preference is persisted in `localStorage` and system mode is detected automatically.

### CSS custom properties

All components use `--pt-*` CSS variables defined in `styles.scss`. They switch automatically in dark mode via the `.p-dark` selector.

**Available variables:**

| Variable                | Light             | Dark              |
|------------------------|--------------------|--------------------|
| `--pt-bg`              | `#f5f5f5`          | `#0f172a`          |
| `--pt-surface`         | `#ffffff`          | `#1e293b`          |
| `--pt-surface-alt`     | `#f8fafc`          | `#1a2332`          |
| `--pt-border`          | `#e2e8f0`          | `#334155`          |
| `--pt-text`            | `#1a1a2e`          | `#f1f5f9`          |
| `--pt-text-secondary`  | `#64748b`          | `#94a3b8`          |
| `--pt-text-muted`      | `#94a3b8`          | `#64748b`          |
| `--pt-primary`         | `#4ade80`          | (unchanged)        |
| `--pt-danger`          | `#ef4444`          | (unchanged)        |
| `--pt-success`         | `#22c55e`          | (unchanged)        |
| `--pt-warning`         | `#f59e0b`          | (unchanged)        |
| `--pt-info`            | `#3b82f6`          | (unchanged)        |

---

### 3. Theme-aware color guide

The most common issue is **hardcoded colors** — a dark hex like `#1a1a2e` that looks fine on a white background becomes invisible on a dark surface. The rule is simple: **never use a raw hex/rgb value for text or backgrounds. Always use a `--pt-*` variable.**

#### Quick reference — which variable to use

| You want to style...              | Use this variable          | Never use            |
|-----------------------------------|----------------------------|----------------------|
| Page background                   | `--pt-bg`                  | `#f5f5f5`, `white`   |
| Card / panel / section background | `--pt-surface`             | `#fff`, `#ffffff`    |
| Alternate row / hover background  | `--pt-surface-alt`         | `#f8fafc`, `#fafafa` |
| Primary body text                 | `--pt-text`                | `#1a1a2e`, `#333`    |
| Secondary / descriptive text      | `--pt-text-secondary`      | `#64748b`, `#666`    |
| Placeholder / disabled text       | `--pt-text-muted`          | `#999`, `#aaa`       |
| Borders / dividers                | `--pt-border`              | `#e2e8f0`, `#ddd`    |
| Accent / links / active state     | `--pt-primary`             | `#4ade80`            |
| Error text / icon                 | `--pt-danger`              | `#ef4444`, `red`     |
| Success text / icon               | `--pt-success`             | `#22c55e`, `green`   |
| Warning text / icon               | `--pt-warning`             | `#f59e0b`, `orange`  |
| Info text / icon                  | `--pt-info`                | `#3b82f6`, `blue`    |

#### Complete component example

```scss
// my-card.component.scss

:host {
  display: block;
  background: var(--pt-surface);         // white in light, slate in dark
  border: 1px solid var(--pt-border);    // adapts automatically
  border-radius: var(--pt-radius);
  padding: 1.5rem;
}

.card-title {
  color: var(--pt-text);                 // dark text in light, light text in dark
  font-weight: 700;
  font-size: 1.125rem;
}

.card-subtitle {
  color: var(--pt-text-secondary);       // medium contrast in both modes
  font-size: 0.875rem;
  margin-top: 0.25rem;
}

.card-body {
  color: var(--pt-text);                 // always readable on --pt-surface
  margin-top: 1rem;
}

.card-footer {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid var(--pt-border);
  color: var(--pt-text-muted);           // de-emphasized text
  font-size: 0.8rem;
}

.card-tag {
  background: var(--pt-primary);
  color: var(--pt-primary-text);         // guaranteed readable on primary bg
  padding: 0.25rem 0.5rem;
  border-radius: var(--pt-radius-full);
  font-size: 0.75rem;
}

.card-error {
  color: var(--pt-danger);               // red that works on any surface
}
```

#### Common mistakes and fixes

**1. Hardcoded text color**

```scss
// BAD — invisible in dark mode
.label { color: #333; }

// GOOD
.label { color: var(--pt-text); }
```

**2. Hardcoded background**

```scss
// BAD — bright white panel in dark mode
.panel { background: #fff; }

// GOOD
.panel { background: var(--pt-surface); }
```

**3. Hardcoded border**

```scss
// BAD — too light in dark, too dark in light
.separator { border-bottom: 1px solid #e0e0e0; }

// GOOD
.separator { border-bottom: 1px solid var(--pt-border); }
```

**4. Feedback color on background**

```scss
// BAD — raw red on unknown background
.error-box { background: #fee2e2; color: #991b1b; }

// GOOD — uses variables that adapt to dark mode
.error-box { background: var(--pt-danger-bg); color: var(--pt-danger); }
```

**5. Using `black` or `white` directly**

```scss
// BAD — always black regardless of theme
h1 { color: black; }

// BAD — always white regardless of theme
.overlay-text { color: white; }

// GOOD — adapts to current theme
h1 { color: var(--pt-text); }

// OK — white is fine if the background is explicitly controlled
// e.g. on a colored button where you own both bg and text
.btn-danger { background: var(--pt-danger); color: #fff; }
```

**6. Inline styles in templates**

```html
<!-- BAD — bypasses the theme system entirely -->
<p style="color: #333;">Some text</p>

<!-- GOOD — use a class -->
<p class="text-secondary">Some text</p>
```

#### Text hierarchy cheat sheet

Use these consistently to build readable pages in both modes:

```html
<div class="page">
  <!-- Level 1: Page title — bold, full contrast -->
  <h1 class="page-title">Dashboard</h1>

  <!-- Level 2: Subtitle / description — medium contrast -->
  <p class="page-subtitle">Overview of your recent activity</p>

  <!-- Level 3: Body text — full contrast, normal weight -->
  <p style="color: var(--pt-text);">This is the main content.</p>

  <!-- Level 4: Secondary info — reduced contrast -->
  <span class="text-secondary">Last updated 2 hours ago</span>

  <!-- Level 5: Muted / disabled — lowest contrast -->
  <span class="text-muted">No data available</span>

  <!-- Accent: draw attention -->
  <span class="text-primary">3 new notifications</span>

  <!-- Status colors -->
  <span class="text-success">Confirmed</span>
  <span class="text-danger">Cancelled</span>
</div>
```

Mapped to CSS variables:

| Level              | Variable              | Class alternative    |
|--------------------|-----------------------|----------------------|
| Title / body text  | `--pt-text`           | —                    |
| Subtitle / labels  | `--pt-text-secondary` | `.text-secondary`    |
| Muted / disabled   | `--pt-text-muted`     | `.text-muted`        |
| Brand accent       | `--pt-primary`        | `.text-primary`      |
| Error              | `--pt-danger`         | `.text-danger`       |
| Success            | `--pt-success`        | `.text-success`      |

#### PrimeNG components

PrimeNG components (buttons, tables, dialogs, inputs...) use PrimeNG's own token system which already adapts to dark mode via the Aura preset. You do **not** need to restyle them with `--pt-*` variables.

However, any **custom wrapper or label** you write around a PrimeNG component should use the theme variables:

```html
<div class="card">
  <!-- Your custom label — use theme vars -->
  <label class="form-label">Select a date</label>

  <!-- PrimeNG component — handles its own theming -->
  <p-calendar />

  <!-- Your custom helper text — use theme vars -->
  <small class="text-muted">Pick a weekday</small>
</div>
```

---

### Responsive mobile-first

#### Breakpoints

| Name  | Min-width | Usage              |
|-------|----------|--------------------|
| `xs`  | 0        | Mobile (default)   |
| `sm`  | 640px    | Large mobile       |
| `md`  | 768px    | Tablet             |
| `lg`  | 1024px   | Desktop            |
| `xl`  | 1280px   | Large desktop      |
| `2xl` | 1536px   | Ultra-wide         |

#### SCSS media queries

Write CSS mobile-first: the default style is mobile, then add media queries for larger screens.

```scss
.my-grid {
  display: grid;
  grid-template-columns: 1fr;           // mobile: 1 column
  gap: 1rem;

  @media (min-width: 768px) {           // tablet: 2 columns
    grid-template-columns: repeat(2, 1fr);
  }

  @media (min-width: 1024px) {          // desktop: 3 columns
    grid-template-columns: repeat(3, 1fr);
  }
}
```

#### Responsive utility classes

```html
<!-- Responsive grid -->
<div class="grid grid--md-2 grid--lg-3">
  <div class="card">...</div>
  <div class="card">...</div>
  <div class="card">...</div>
</div>

<!-- Responsive visibility -->
<div class="hide-mobile">Desktop only</div>
<div class="hide-desktop">Mobile only</div>

<!-- Vertical stack -->
<div class="stack">
  <div>...</div>
  <div>...</div>
</div>

<!-- Horizontal wrap -->
<div class="cluster">
  <span class="badge badge-success">OK</span>
  <span class="badge badge-info">Info</span>
</div>
```

#### BreakpointService (TypeScript)

For conditional logic in TypeScript or templates:

```typescript
private readonly bp = inject(BreakpointService);

// In template
@if (bp.isMobile()) {
  <app-mobile-layout />
} @else {
  <app-desktop-layout />
}

// Programmatic check
if (this.bp.isAtLeast('lg')) {
  // desktop-only logic
}

// Current breakpoint
this.bp.current()  // 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl'

// Individual signals
this.bp.md()       // true if viewport >= 768px
this.bp.lg()       // true if viewport >= 1024px
```

### Using PrimeNG

PrimeNG components automatically follow the Aura theme and dark mode (via `.p-dark`). Import directly in standalone components:

```typescript
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';

@Component({
  standalone: true,
  imports: [ButtonModule, TableModule, DialogModule],
  template: `
    <p-button label="Action" severity="success" />
    <p-table [value]="items()" [paginator]="true" [rows]="10">
      ...
    </p-table>
  `,
})
```

---

## Dev

```bash
npm start        # Dev server
npm run build    # Production build
npm test         # Tests (Vitest)
```
