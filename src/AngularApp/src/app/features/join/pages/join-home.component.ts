import {Component} from '@angular/core';
import {PageShellComponent} from '@shared/components/page-shell/page-shell.component';

@Component({
  selector: 'app-join-home',
  standalone: true,
  imports: [PageShellComponent],
  template: `
    <app-page-shell
      eyebrow="Rejoindre"
      title="Rejoindre votre"
      titleEm="prochain match"
      subtitle="Choisissez un match à rejoindre"
      dividerLabel="disponibilités en temps réel">
    </app-page-shell>
  `
})
export class JoinHomeComponent {
}
