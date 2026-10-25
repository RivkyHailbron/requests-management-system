export const requestStatuses = ['New', 'InProgress', 'Waiting', 'Completed'] as const;
export type RequestStatus = (typeof requestStatuses)[number];

export const requestStatusLabels: Readonly<Record<RequestStatus, string>> = {
  New: 'New',
  InProgress: 'In progress',
  Waiting: 'Waiting',
  Completed: 'Completed',
};

export const requestPriorities = ['Low', 'Medium', 'High'] as const;
export type RequestPriority = (typeof requestPriorities)[number];

export const requestSortFields = ['CreatedAt', 'UpdatedAt', 'Priority', 'Status', 'Title', 'OrganizationName'] as const;
export type RequestSortField = (typeof requestSortFields)[number];

export type SortDirection = 'Asc' | 'Desc';

export interface RequestItem {
  readonly id: number;
  readonly title: string;
  readonly organizationName: string;
  readonly status: RequestStatus;
  readonly priority: RequestPriority;
  readonly assignedTo: string | null;
  readonly createdAt: string;
  readonly updatedAt: string;
  readonly version: string;
}

export interface PagedResult<T> {
  readonly items: readonly T[];
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
  readonly totalPages: number;
}

export interface RequestFilters {
  readonly search: string;
  readonly statuses: readonly RequestStatus[];
  readonly priorities: readonly RequestPriority[];
  readonly organizationName: string;
  readonly assignedTo: string;
  readonly createdFrom: string;
  readonly createdTo: string;
}

export interface RequestQuery extends RequestFilters {
  readonly page: number;
  readonly pageSize: number;
  readonly sortBy: RequestSortField;
  readonly sortDirection: SortDirection;
}

export interface StatusCount {
  readonly status: RequestStatus;
  readonly count: number;
}

export interface PriorityCount {
  readonly priority: RequestPriority;
  readonly count: number;
}

export interface RequestsSummary {
  readonly totalCount: number;
  readonly byStatus: readonly StatusCount[];
  readonly byPriority: readonly PriorityCount[];
}

export interface StatusHistoryEntry {
  readonly id: number;
  readonly requestId: number;
  readonly previousStatus: RequestStatus;
  readonly newStatus: RequestStatus;
  readonly changedAt: string;
  readonly changedBy: string;
}

export type StatusTransitions = Readonly<Record<RequestStatus, readonly RequestStatus[]>>;

export type StatusUpdateOutcome = 'Updated' | 'NotFound' | 'Conflict' | 'InvalidTransition';

export interface BulkStatusUpdateItemResult {
  readonly id: number;
  readonly outcome: StatusUpdateOutcome;
  readonly request: RequestItem | null;
}

export interface BulkStatusUpdateResult {
  readonly requestedCount: number;
  readonly updatedCount: number;
  readonly failedCount: number;
  readonly items: readonly BulkStatusUpdateItemResult[];
}

export const emptyFilters: RequestFilters = {
  search: '',
  statuses: [],
  priorities: [],
  organizationName: '',
  assignedTo: '',
  createdFrom: '',
  createdTo: '',
};
