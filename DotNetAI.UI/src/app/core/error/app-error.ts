export interface AppError {
  readonly title: string;
  readonly message: string;
  readonly status?: number;
}
