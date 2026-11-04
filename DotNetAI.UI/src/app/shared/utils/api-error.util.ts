export function getApiErrorMessage(error: unknown): string {
  if (typeof error !== 'object' || error === null) {
    return 'The API request failed.';
  }

  const err = error as Record<string, unknown>;
  const errorBody = err['error'];

  if (typeof errorBody === 'object' && errorBody !== null) {
    const body = errorBody as Record<string, unknown>;
    if (typeof body['error'] === 'string') return body['error'];
    if (typeof body['message'] === 'string') return body['message'];
  }

  if (typeof err['statusText'] === 'string') return err['statusText'];

  return 'The API request failed.';
}
