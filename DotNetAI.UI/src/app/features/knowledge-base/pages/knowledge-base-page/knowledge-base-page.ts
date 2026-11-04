import { Component, inject, OnInit, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';

import { normalizeHttpError } from '../../../../core/error/error-normalizer';
import { KnowledgeApiService } from '../../data-access/knowledge-api.service';
import { KnowledgeDocument } from '../../models/knowledge-document.model';

@Component({
  selector: 'app-knowledge-base-page',
  standalone: true,
  imports: [
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule,
    MatTableModule,
  ],
  templateUrl: './knowledge-base-page.html',
  styleUrl: './knowledge-base-page.scss',
})
export class KnowledgeBasePage implements OnInit {
  private readonly api = inject(KnowledgeApiService);

  readonly documents = signal<KnowledgeDocument[]>([]);
  readonly loading = signal(true);
  readonly uploading = signal(false);
  readonly error = signal('');
  readonly displayedColumns = ['fileName', 'documentId', 'chunkCount'];

  ngOnInit(): void {
    this.loadDocuments();
  }

  loadDocuments(): void {
    this.loading.set(true);
    this.error.set('');
    this.api
      .listDocuments()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (documents) => this.documents.set(documents),
        error: (error) => this.error.set(normalizeHttpError(error).message),
      });
  }

  upload(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.item(0);
    if (!file) {
      return;
    }

    this.uploading.set(true);
    this.error.set('');
    this.api
      .upload(file)
      .pipe(finalize(() => this.uploading.set(false)))
      .subscribe({
        next: () => {
          input.value = '';
          this.loadDocuments();
        },
        error: (error) => this.error.set(normalizeHttpError(error).message),
      });
  }
}
