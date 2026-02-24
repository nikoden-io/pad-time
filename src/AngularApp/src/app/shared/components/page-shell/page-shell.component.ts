import {
  ChangeDetectionStrategy, Component, Input,
} from '@angular/core';
import {CommonModule} from '@angular/common';

@Component({
  selector: 'app-page-shell',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './page-shell.component.html',
  styleUrls: ['./page-shell.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageShellComponent {
  @Input() eyebrow = '';
  @Input() title = '';
  @Input() titleEm = '';
  @Input() subtitle = '';
  @Input() dividerLabel = '';
}
