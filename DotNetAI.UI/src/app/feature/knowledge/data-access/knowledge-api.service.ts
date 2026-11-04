import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../../../core/config/api.config';
import {
  DocumentIngestionResult,
  KnowledgeDocument,
  KnowledgeSearchResponse
} from '../models/knowledge.models';

@Injectable({ providedIn: 'root' })
export class KnowledgeApiService {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(API_CONFIG);

  getDocuments(): Observable<KnowledgeDocument[]> {
    return this.http.get<KnowledgeDocument[]>(`${this.apiConfig.knowledgeApiUrl}/documents`);
  }

  uploadDocument(file: File): Observable<DocumentIngestionResult> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<DocumentIngestionResult>(
      `${this.apiConfig.knowledgeApiUrl}/documents`,
      formData
    );
  }

  search(query: string, topK: number): Observable<KnowledgeSearchResponse> {
    return this.http.post<KnowledgeSearchResponse>(
      `${this.apiConfig.knowledgeApiUrl}/search`,
      { query, topK }
    );
  }
}
