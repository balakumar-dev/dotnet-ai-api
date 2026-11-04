import { HttpErrorResponse } from '@angular/common/http';

import { AppError } from './app-error';

export function normalizeHttpError(error: unknown): AppError {
  if (error instanceof HttpErrorResponse) {
    return {
      title: error.status === 0 ? 'Service unavailable' : 'Request failed',
      message:
        typeof error.error?.message === 'string'
          ? error.error.message
          : error.status === 0
            ? 'The service could not be reached.'
            : 'The requested operation could not be completed.',
      status: error.status,
    };
  }

  return {
    title: 'Unexpected error',
    message: 'An unexpected error occurred.',
  };
}
