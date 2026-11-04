import { DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { normalizeHttpError } from '../../../../core/error/error-normalizer';
import { SemanticSearchApiService } from '../../data-access/semantic-search-api.service';
import { KnowledgeSearchMatch } from '../../models/semantic-search.model';

@Component({
  selector: 'app-semantic-search-page',
  standalone: true,
  imports: [
    DecimalPipe,
    FormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressBarModule,
  ],
  templateUrl: './semantic-search-page.html',
  styleUrl: './semantic-search-page.scss',
})
export class SemanticSearchPage {
  private readonly api = inject(SemanticSearchApiService);

  readonly results = signal<KnowledgeSearchMatch[]>([]);
  readonly searching = signal(false);
  readonly searched = signal(false);
  readonly error = signal('');
  query = '';
  topK = 3;

  search(): void {
    const query = this.query.trim();
    if (!query || this.searching()) {
      return;
    }

    this.searching.set(true);
    this.searched.set(true);
    this.error.set('');
    this.api
      .search({ query, topK: this.topK })
      .pipe(finalize(() => this.searching.set(false)))
      .subscribe({
        next: (response) => this.results.set(response.results),
        error: (error) => this.error.set(normalizeHttpError(error).message),
      });
  }
}
