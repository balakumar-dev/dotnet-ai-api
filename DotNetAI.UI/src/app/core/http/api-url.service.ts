import { inject, Injectable } from '@angular/core';

import { API_CONFIG } from '../config/api-config.token';

@Injectable({ providedIn: 'root' })
export class ApiUrlService {
  private readonly config = inject(API_CONFIG);

  resolve(path: string): string {
    const baseUrl = this.config.gatewayUrl ?? this.config.baseUrl;
    return `${baseUrl.replace(/\/$/, '')}/${path.replace(/^\//, '')}`;
  }
}
