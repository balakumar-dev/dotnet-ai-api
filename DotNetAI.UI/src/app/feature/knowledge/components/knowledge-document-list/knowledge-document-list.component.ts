import { Component, DestroyRef, EventEmitter, OnInit, Output, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { KnowledgeApiService } from '../../data-access/knowledge-api.service';
import { KnowledgeDocument } from '../../models/knowledge.models';
import { getApiErrorMessage } from '../../../../shared/utils/api-error.util';

@Component({
  selector: 'app-knowledge-document-list',
  imports: [],
  templateUrl: './knowledge-document-list.component.html',
  styleUrl: './knowledge-document-list.component.css'
})
export class KnowledgeDocumentListComponent implements OnInit {
  private readonly api = inject(KnowledgeApiService);
  private readonly destroyRef = inject(DestroyRef);

  @Output() failed = new EventEmitter<string>();

  protected documents: KnowledgeDocument[] = [];
  protected loading = false;

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading = true;
    this.api.getDocuments().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: documents => {
        this.documents = documents;
        this.loading = false;
      },
      error: error => {
        this.failed.emit(getApiErrorMessage(error));
        this.loading = false;
      }
    });
  }
}
