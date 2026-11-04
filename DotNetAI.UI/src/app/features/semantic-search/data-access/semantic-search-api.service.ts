import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiUrlService } from '../../../core/http/api-url.service';
import {
  KnowledgeSearchRequest,
  KnowledgeSearchResponse,
} from '../models/semantic-search.model';

@Injectable({ providedIn: 'root' })
export class SemanticSearchApiService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(ApiUrlService);

  search(request: KnowledgeSearchRequest): Observable<KnowledgeSearchResponse> {
    return this.http.post<KnowledgeSearchResponse>(
      this.apiUrl.resolve('knowledge/search'),
      request,
    );
  }
}
