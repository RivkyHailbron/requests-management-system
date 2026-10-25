import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { APP_CONFIG } from '../../../core/config/app-config';
import {
  BulkStatusUpdateResult,
  PagedResult,
  RequestItem,
  RequestQuery,
  RequestStatus,
  RequestsSummary,
  StatusHistoryEntry,
  StatusTransitions,
} from '../models/request.models';

const startOfDaySuffix = 'T00:00:00.000Z';
const endOfDaySuffix = 'T23:59:59.999Z';

@Injectable({ providedIn: 'root' })
export class RequestsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${inject(APP_CONFIG).apiBaseUrl}/requests`;

  getRequests(query: RequestQuery): Observable<PagedResult<RequestItem>> {
    return this.http.get<PagedResult<RequestItem>>(this.baseUrl, { params: toHttpParams(query) });
  }

  getSummary(): Observable<RequestsSummary> {
    return this.http.get<RequestsSummary>(`${this.baseUrl}/summary`);
  }

  getOrganizationNames(search: string): Observable<readonly string[]> {
    return this.getLookup('organizations', search);
  }

  getAssignees(search: string): Observable<readonly string[]> {
    return this.getLookup('assignees', search);
  }

  getStatusTransitions(): Observable<StatusTransitions> {
    return this.http.get<StatusTransitions>(`${this.baseUrl}/status-transitions`);
  }

  getHistory(requestId: number): Observable<readonly StatusHistoryEntry[]> {
    return this.http.get<readonly StatusHistoryEntry[]>(`${this.baseUrl}/${requestId}/history`);
  }

  updateStatus(requestId: number, status: RequestStatus, version: string): Observable<RequestItem> {
    return this.http.patch<RequestItem>(`${this.baseUrl}/${requestId}/status`, { status, version });
  }

  bulkUpdateStatus(status: RequestStatus, requests: readonly RequestItem[]): Observable<BulkStatusUpdateResult> {
    return this.http.patch<BulkStatusUpdateResult>(`${this.baseUrl}/status`, {
      status,
      items: requests.map(({ id, version }) => ({ id, version })),
    });
  }

  private getLookup(lookup: string, search: string): Observable<readonly string[]> {
    const params = search ? new HttpParams().set('search', search) : undefined;
    return this.http.get<readonly string[]>(`${this.baseUrl}/lookups/${lookup}`, { params });
  }
}

function toHttpParams(query: RequestQuery): HttpParams {
  let params = new HttpParams()
    .set('page', query.page)
    .set('pageSize', query.pageSize)
    .set('sortBy', query.sortBy)
    .set('sortDirection', query.sortDirection);

  const textFilters = {
    search: query.search,
    organizationName: query.organizationName,
    assignedTo: query.assignedTo,
    createdFrom: query.createdFrom && query.createdFrom + startOfDaySuffix,
    createdTo: query.createdTo && query.createdTo + endOfDaySuffix,
  };

  for (const [key, value] of Object.entries(textFilters)) {
    const trimmed = value.trim();
    if (trimmed) {
      params = params.set(key, trimmed);
    }
  }

  query.statuses.forEach((status) => (params = params.append('status', status)));
  query.priorities.forEach((priority) => (params = params.append('priority', priority)));

  return params;
}
