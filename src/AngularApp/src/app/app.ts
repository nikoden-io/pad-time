// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/**
 * @description Root application component that serves as the entry point for the PadTime Angular app.
 * Renders the top-level router outlet to display routed views.
 */
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `<router-outlet />`,
})
export class AppComponent {}