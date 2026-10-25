import { DestroyRef, Injectable, Signal, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import {
  BehaviorSubject,
  Observable,
  Subject,
  catchError,
  debounceTime,
  distinctUntilChanged,
  map,
  merge,
  of,
  switchMap,
  tap,
} from 'rxjs';
import { APP_CONFIG } from '../../../core/config/app-config';
import { toErrorMessage } from '../../../core/http/problem-details';
import { RequestsApi } from '../data/requests-api';
import {
  BulkStatusUpdateResult,
  PagedResult,
  RequestFilters,
  RequestItem,
  RequestQuery,
  RequestSortField,
  RequestStatus,
  RequestsSummary,
  SortDirection,
  StatusHistoryEntry,
  StatusTransitions,
  emptyFilters,
} from '../models/request.models';

interface ListState {
  readonly page: PagedResult<RequestItem> | null;
  readonly loading: boolean;
  readonly error: string | null;
}

interface Lookup {
  readonly options: Signal<readonly string[]>;
  readonly search: (term: string) => void;
}

interface SummaryState {
  readonly summary: RequestsSummary | null;
  readonly error: string | null;
}

@Injectable()
export class RequestsStore {
  private readonly api = inject(RequestsApi);
  private readonly config = inject(APP_CONFIG);
  private readonly destroyRef = inject(DestroyRef);

  private readonly querySubject = new BehaviorSubject<RequestQuery>({
    ...emptyFilters,
    page: 1,
    pageSize: this.config.defaultPageSize,
    sortBy: 'CreatedAt',
    sortDirection: 'Desc',
  });
  private readonly filtersSubject = new Subject<RequestFilters>();
  private readonly reloadSubject = new Subject<void>();
  private readonly summaryRefreshSubject = new BehaviorSubject<void>(undefined);

  private readonly listState = signal<ListState>({ page: null, loading: true, error: null });
  private readonly summaryState = signal<SummaryState>({ summary: null, error: null });
  private readonly organizationLookup = this.createLookup((search) => this.api.getOrganizationNames(search));
  private readonly assigneeLookup = this.createLookup((search) => this.api.getAssignees(search));

  readonly query = toSignal(this.querySubject, { requireSync: true });
  readonly page = computed(() => this.listState().page);
  readonly loading = computed(() => this.listState().loading);
  readonly error = computed(() => this.listState().error);
  readonly isEmpty = computed(() => !this.loading() && !this.error() && this.page()?.totalCount === 0);
  readonly summary = computed(() => this.summaryState().summary);
  readonly summaryError = computed(() => this.summaryState().error);
  readonly organizationOptions = this.organizationLookup.options;
  readonly assigneeOptions = this.assigneeLookup.options;
  readonly transitions = toSignal(
    this.api.getStatusTransitions().pipe(catchError(() => of(null))),
    { initialValue: null as StatusTransitions | null },
  );

  constructor() {
    this.filtersSubject
      .pipe(
        debounceTime(this.config.searchDebounceMs),
        distinctUntilChanged((previous, current) => JSON.stringify(previous) === JSON.stringify(current)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((filters) => this.updateQuery({ ...filters, page: 1 }));

    merge(this.querySubject, this.reloadSubject.pipe(map(() => this.querySubject.value)))
      .pipe(
        tap(() => this.listState.update((state) => ({ ...state, loading: true, error: null }))),
        switchMap((query) =>
          this.api.getRequests(query).pipe(
            map((page): ListState => ({ page, loading: false, error: null })),
            catchError((error: unknown) => of<ListState>({ page: null, loading: false, error: toErrorMessage(error) })),
          ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((state) => this.listState.set(state));

    this.summaryRefreshSubject
      .pipe(
        switchMap(() =>
          this.api.getSummary().pipe(
            map((summary): SummaryState => ({ summary, error: null })),
            catchError((error: unknown) =>
              of<SummaryState>({ summary: this.summaryState().summary, error: toErrorMessage(error) }),
            ),
          ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((state) => this.summaryState.set(state));
  }

  changeFilters(filters: RequestFilters): void {
    this.filtersSubject.next(filters);
  }

  searchOrganizations(term: string): void {
    this.organizationLookup.search(term);
  }

  searchAssignees(term: string): void {
    this.assigneeLookup.search(term);
  }

  changePage(page: number, pageSize: number): void {
    this.updateQuery({ page, pageSize });
  }

  changeSort(sortBy: RequestSortField, sortDirection: SortDirection): void {
    this.updateQuery({ sortBy, sortDirection, page: 1 });
  }

  reload(): void {
    this.reloadSubject.next();
    this.summaryRefreshSubject.next();
  }

  allowedTargets(status: RequestStatus): readonly RequestStatus[] {
    return this.transitions()?.[status] ?? [];
  }

  updateStatus(request: RequestItem, status: RequestStatus): Observable<RequestItem> {
    return this.api.updateStatus(request.id, status, request.version).pipe(tap(() => this.reload()));
  }

  bulkUpdateStatus(status: RequestStatus, requests: readonly RequestItem[]): Observable<BulkStatusUpdateResult> {
    return this.api.bulkUpdateStatus(status, requests).pipe(tap(() => this.reload()));
  }

  history(requestId: number): Observable<readonly StatusHistoryEntry[]> {
    return this.api.getHistory(requestId);
  }

  private createLookup(load: (search: string) => Observable<readonly string[]>): Lookup {
    const terms = new Subject<string>();
    const options = toSignal(
      terms.pipe(
        debounceTime(this.config.searchDebounceMs),
        map((term) => term.trim()),
        distinctUntilChanged(),
        switchMap((term) => load(term).pipe(catchError(() => of<readonly string[]>([])))),
      ),
      { initialValue: [] },
    );

    return { options, search: (term) => terms.next(term) };
  }

  private updateQuery(changes: Partial<RequestQuery>): void {
    this.querySubject.next({ ...this.querySubject.value, ...changes });
  }
}
