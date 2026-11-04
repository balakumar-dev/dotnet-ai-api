import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiUrlService } from '../../../core/http/api-url.service';
import { ChatRequest, ChatResponse, TopicAnalysis } from '../models/chat.model';

@Injectable({ providedIn: 'root' })
export class ChatApiService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(ApiUrlService);

  chat(request: ChatRequest): Observable<ChatResponse> {
    return this.http.post<ChatResponse>(this.apiUrl.resolve('chat'), request);
  }

  knowledgeChat(request: ChatRequest): Observable<ChatResponse> {
    return this.http.post<ChatResponse>(this.apiUrl.resolve('chat/knowledge'), request);
  }

  analyze(request: ChatRequest): Observable<TopicAnalysis> {
    return this.http.post<TopicAnalysis>(this.apiUrl.resolve('chat/analyze'), request);
  }
}
