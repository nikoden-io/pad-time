import {Component} from '@angular/core';
import {RouterLink} from '@angular/router';
import {ButtonModule} from 'primeng/button';
import {TranslocoDirective} from '@jsverse/transloco';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [RouterLink, ButtonModule, TranslocoDirective],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss',
})
export class AdminDashboardComponent {
}
