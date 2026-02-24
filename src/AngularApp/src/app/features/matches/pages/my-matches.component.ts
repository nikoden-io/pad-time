import {Component} from '@angular/core';
import {MyMatchesPageComponent} from '@features/matches/components/my-matches-page/my-matches-page.component';
import {PageShellComponent} from '@shared/components/page-shell/page-shell.component';

@Component({
  selector: 'app-my-matches',
  standalone: true,
  imports: [MyMatchesPageComponent, PageShellComponent],
  template: `
    <app-page-shell
      eyebrow="Mes matchs"
      title="Votre"
      titleEm="historique de matchs"
      subtitle="Consultez et gérez vos réservations."
      dividerLabel="matchs à venir">
      <app-my-matches-page/>
    </app-page-shell>
  `
})
export class MyMatchesComponent {
}
