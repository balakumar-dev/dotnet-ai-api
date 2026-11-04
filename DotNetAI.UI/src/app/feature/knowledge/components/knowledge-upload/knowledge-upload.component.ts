import { Component, DestroyRef, EventEmitter, Output, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { KnowledgeApiService } from '../../data-access/knowledge-api.service';
import { DocumentIngestionResult } from '../../models/knowledge.models';
import { getApiErrorMessage } from '../../../../shared/utils/api-error.util';

const MAX_FILE_SIZE_MB = 20;
const ACCEPTED_EXTENSIONS = ['.pdf', '.txt', '.docx', '.md'];

@Component({
  selector: 'app-knowledge-upload',
  imports: [],
  templateUrl: './knowledge-upload.component.html',
  styleUrl: './knowledge-upload.component.css'
})
export class KnowledgeUploadComponent {
  private readonly api = inject(KnowledgeApiService);
  private readonly destroyRef = inject(DestroyRef);

  @Output() uploaded = new EventEmitter<DocumentIngestionResult>();
  @Output() failed = new EventEmitter<string>();

  protected selectedFile: File | null = null;
  protected uploading = false;

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    if (file) {
      const ext = '.' + file.name.split('.').pop()?.toLowerCase();
      if (!ACCEPTED_EXTENSIONS.includes(ext)) {
        this.failed.emit(`Unsupported file type. Accepted: ${ACCEPTED_EXTENSIONS.join(', ')}`);
        input.value = '';
        return;
      }
      if (file.size > MAX_FILE_SIZE_MB * 1024 * 1024) {
        this.failed.emit(`File exceeds the ${MAX_FILE_SIZE_MB} MB size limit.`);
        input.value = '';
        return;
      }
    }

    this.selectedFile = file;
  }

  protected upload(): void {
    if (!this.selectedFile) {
      this.failed.emit('Select a document to upload.');
      return;
    }

    this.uploading = true;
    this.api.uploadDocument(this.selectedFile).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: result => {
        this.uploaded.emit(result);
        this.selectedFile = null;
        this.uploading = false;
      },
      error: error => {
        this.failed.emit(getApiErrorMessage(error));
        this.uploading = false;
      }
    });
  }
}
