import { InjectionToken } from '@angular/core';

export interface AppConfig {
  readonly apiBaseUrl: string;
  readonly userIdHeader: string;
  readonly defaultUserId: string;
  readonly searchDebounceMs: number;
  readonly defaultPageSize: number;
  readonly pageSizeOptions: readonly number[];
  readonly maxBulkItems: number;
}

export const APP_CONFIG = new InjectionToken<AppConfig>('APP_CONFIG');

export const defaultAppConfig: AppConfig = {
  apiBaseUrl: '/api',
  userIdHeader: 'X-User-Id',
  defaultUserId: 'demo.user',
  searchDebounceMs: 400,
  defaultPageSize: 20,
  pageSizeOptions: [10, 20, 50, 100],
  maxBulkItems: 100,
};
