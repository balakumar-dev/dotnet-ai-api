import { Component, input } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-page-state',
  standalone: true,
  imports: [MatProgressSpinnerModule],
  templateUrl: './page-state.html',
  styleUrl: './page-state.scss',
})
export class PageState {
  readonly loading = input(false);
  readonly title = input<string>();
  readonly message = input<string>();
}
