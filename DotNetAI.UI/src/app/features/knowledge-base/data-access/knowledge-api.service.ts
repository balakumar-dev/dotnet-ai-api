import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiUrlService } from '../../../core/http/api-url.service';
import {
  DocumentIngestionResult,
  KnowledgeDocument,
} from '../models/knowledge-document.model';

@Injectable({ providedIn: 'root' })
export class KnowledgeApiService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(ApiUrlService);

  listDocuments(): Observable<KnowledgeDocument[]> {
    return this.http.get<KnowledgeDocument[]>(
      this.apiUrl.resolve('knowledge/documents'),
    );
  }

  upload(file: File): Observable<DocumentIngestionResult> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<DocumentIngestionResult>(
      this.apiUrl.resolve('knowledge/documents'),
      formData,
    );
  }
}
