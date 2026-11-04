import { Provider } from '@angular/core';
import { environment } from '../../../environments/environment';
import { API_CONFIG, ApiConfig } from './api.config';

export const API_CONFIG_PROVIDER: Provider = {
  provide: API_CONFIG,
  useValue: {
    knowledgeApiUrl: environment.knowledgeApiUrl
  } satisfies ApiConfig
};
