import { ChangeDetectionStrategy, Component, DestroyRef, Injector, computed, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DecimalPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { APP_CONFIG } from '../../../core/config/app-config';
import { toErrorMessage } from '../../../core/http/problem-details';
import { StateMessage } from '../../../shared/state-message/state-message';
import { BulkStatusBar } from '../bulk-status-bar/bulk-status-bar';
import { BulkStatusUpdateResult, RequestItem, RequestStatus, StatusUpdateOutcome } from '../models/request.models';
import { RequestHistoryDialog } from '../request-history-dialog/request-history-dialog';
import { RequestsFilters } from '../requests-filters/requests-filters';
import { RequestsSummary } from '../requests-summary/requests-summary';
import { RequestsTable, SortChange } from '../requests-table/requests-table';
import { RequestsStore } from '../state/requests-store';
import { StatusUpdateDialog } from '../status-update-dialog/status-update-dialog';

const outcomeLabels: Readonly<Record<Exclude<StatusUpdateOutcome, 'Updated'>, string>> = {
  Conflict: 'changed by another user',
  InvalidTransition: 'transition not allowed',
  NotFound: 'not found',
};

@Component({
  selector: 'app-requests-page',
  imports: [
    BulkStatusBar,
    DecimalPipe,
    MatCardModule,
    MatPaginatorModule,
    MatProgressBarModule,
    RequestsFilters,
    RequestsSummary,
    RequestsTable,
    StateMessage,
  ],
  providers: [RequestsStore],
  templateUrl: './requests-page.html',
  styleUrl: './requests-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RequestsPage {
  protected readonly store = inject(RequestsStore);
  protected readonly config = inject(APP_CONFIG);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly injector = inject(Injector);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly selected = signal<readonly RequestItem[]>([]);
  protected readonly selectedIds = computed(() => new Set(this.selected().map((request) => request.id)));
  protected readonly bulkBusy = signal(false);

  constructor() {
    effect(() => {
      this.store.page();
      this.selected.set([]);
    });
  }

  protected onSortChange({ sortBy, sortDirection }: SortChange): void {
    this.store.changeSort(sortBy, sortDirection);
  }

  protected onPageChange(event: PageEvent): void {
    this.store.changePage(event.pageIndex + 1, event.pageSize);
  }

  protected openStatusDialog(request: RequestItem): void {
    this.dialog
      .open(StatusUpdateDialog, { data: request, injector: this.injector, width: '480px' })
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((updated: RequestItem | undefined) => {
        if (updated) {
          this.snackBar.open(`Request #${updated.id} moved to ${updated.status}.`, undefined, { duration: 3000 });
        }
      });
  }

  protected openHistoryDialog(request: RequestItem): void {
    this.dialog.open(RequestHistoryDialog, { data: request, injector: this.injector, width: '560px' });
  }

  protected applyBulkStatus(status: RequestStatus): void {
    this.bulkBusy.set(true);
    this.store
      .bulkUpdateStatus(status, this.selected())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.bulkBusy.set(false);
          this.snackBar.open(describeBulkResult(result), 'Close');
        },
        error: (error: unknown) => {
          this.bulkBusy.set(false);
          this.snackBar.open(toErrorMessage(error), 'Close');
        },
      });
  }
}

function describeBulkResult(result: BulkStatusUpdateResult): string {
  const failures = (Object.keys(outcomeLabels) as (keyof typeof outcomeLabels)[])
    .map((outcome) => ({ outcome, count: result.items.filter((item) => item.outcome === outcome).length }))
    .filter(({ count }) => count > 0)
    .map(({ outcome, count }) => `${count} ${outcomeLabels[outcome]}`);

  const summary = `Updated ${result.updatedCount} of ${result.requestedCount}.`;
  return failures.length > 0 ? `${summary} Not updated: ${failures.join(', ')}.` : summary;
}
