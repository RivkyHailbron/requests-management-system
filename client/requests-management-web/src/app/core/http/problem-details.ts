import { HttpErrorResponse } from '@angular/common/http';

export interface ProblemDetails {
  readonly title?: string;
  readonly detail?: string;
  readonly status?: number;
  readonly errors?: Readonly<Record<string, readonly string[]>>;
  readonly [extension: string]: unknown;
}

const unreachableServerMessage = 'The server is unreachable. Check your connection and try again.';
const unexpectedErrorMessage = 'An unexpected error occurred. Please try again.';

export function toProblemDetails(error: unknown): ProblemDetails | null {
  return error instanceof HttpErrorResponse && error.error && typeof error.error === 'object'
    ? (error.error as ProblemDetails)
    : null;
}

export function toErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse && error.status === 0) {
    return unreachableServerMessage;
  }

  const problem = toProblemDetails(error);
  const validationErrors = Object.values(problem?.errors ?? {}).flat();

  return validationErrors.length > 0
    ? validationErrors.join(' ')
    : (problem?.detail ?? problem?.title ?? unexpectedErrorMessage);
}
