import { DecimalPipe } from '@angular/common';
import { Component, DestroyRef, EventEmitter, Output, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { KnowledgeApiService } from '../../data-access/knowledge-api.service';
import { KnowledgeSearchMatch } from '../../models/knowledge.models';
import { getApiErrorMessage } from '../../../../shared/utils/api-error.util';

@Component({
  selector: 'app-knowledge-search',
  imports: [FormsModule, DecimalPipe],
  templateUrl: './knowledge-search.component.html',
  styleUrl: './knowledge-search.component.css'
})
export class KnowledgeSearchComponent {
  private readonly api = inject(KnowledgeApiService);
  private readonly destroyRef = inject(DestroyRef);

  @Output() failed = new EventEmitter<string>();

  protected query = '';
  protected topK = 3;
  protected searching = false;
  protected hasSearched = false;
  protected matches: KnowledgeSearchMatch[] = [];

  protected search(): void {
    if (!this.query.trim()) {
      this.failed.emit('Enter a search query.');
      return;
    }

    this.searching = true;
    this.matches = [];
    this.api.search(this.query.trim(), this.topK).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: response => {
        this.matches = response.results;
        this.hasSearched = true;
        this.searching = false;
      },
      error: error => {
        this.failed.emit(getApiErrorMessage(error));
        this.hasSearched = true;
        this.searching = false;
      }
    });
  }
}
