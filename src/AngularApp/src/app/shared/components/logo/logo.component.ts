import {ChangeDetectionStrategy, Component, Input} from '@angular/core';

@Component({
  selector: 'app-logo',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <svg
      [attr.width]="size"
      [attr.height]="size"
      viewBox="0 0 56 56"
      fill="none"
      [attr.aria-label]="'Pad Time Logo'"
      role="img"
    >
      <defs>
        <filter id="logo-glow">
          <feGaussianBlur stdDeviation="1.5" result="blur"/>
          <feMerge>
            <feMergeNode in="blur"/>
            <feMergeNode in="SourceGraphic"/>
          </feMerge>
        </filter>
      </defs>

      <!-- Racket frame -->
      <ellipse
        cx="26" cy="22" rx="14" ry="16"
        stroke="#4ade80" stroke-width="2.8"
        stroke-linecap="round" fill="none"
        filter="url(#logo-glow)"
      />

      <!-- Strings -->
      <g opacity="0.5">
        <line x1="13" y1="16" x2="39" y2="16" stroke="#4ade80" stroke-width="0.9"/>
        <line x1="12" y1="22" x2="40" y2="22" stroke="#4ade80" stroke-width="0.9"/>
        <line x1="13" y1="28" x2="39" y2="28" stroke="#4ade80" stroke-width="0.9"/>
        <line x1="20" y1="7" x2="20" y2="37" stroke="#4ade80" stroke-width="0.9"/>
        <line x1="26" y1="6" x2="26" y2="38" stroke="#4ade80" stroke-width="0.9"/>
        <line x1="32" y1="7" x2="32" y2="37" stroke="#4ade80" stroke-width="0.9"/>
      </g>

      <!-- Handle -->
      <line
        x1="26" y1="36" x2="34" y2="50"
        stroke="#4ade80" stroke-width="3.2"
        stroke-linecap="round"
        filter="url(#logo-glow)"
      />

      <!-- Ball -->
      <circle
        cx="43" cy="10" r="4.5"
        fill="transparent" stroke="#4ade80" stroke-width="2.2"
        filter="url(#logo-glow)"
      />
      <path
        d="M40.5 8.5 Q43 7 45.5 9.5"
        stroke="#4ade80" stroke-width="1"
        stroke-linecap="round" fill="none" opacity="0.6"
      />
    </svg>
  `,
  styles: [':host { display: inline-flex; align-items: center; }'],
})
export class LogoComponent {
  @Input() size: number | string = 24;
}
