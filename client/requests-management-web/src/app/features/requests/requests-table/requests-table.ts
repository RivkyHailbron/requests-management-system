import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RequestItem, RequestSortField, SortDirection, requestSortFields } from '../models/request.models';

export interface SortChange {
  readonly sortBy: RequestSortField;
  readonly sortDirection: SortDirection;
}

@Component({
  selector: 'app-requests-table',
  imports: [DatePipe, MatButtonModule, MatCheckboxModule, MatIconModule, MatSortModule, MatTableModule, MatTooltipModule],
  templateUrl: './requests-table.html',
  styleUrl: './requests-table.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RequestsTable {
  protected readonly columns = [
    'select',
    'Id',
    'Title',
    'OrganizationName',
    'Status',
    'Priority',
    'AssignedTo',
    'CreatedAt',
    'UpdatedAt',
    'actions',
  ];

  readonly requests = input.required<readonly RequestItem[]>();
  readonly sortBy = input.required<RequestSortField>();
  readonly sortDirection = input.required<SortDirection>();
  readonly selectedIds = input.required<ReadonlySet<number>>();

  readonly sortChange = output<SortChange>();
  readonly selectionChange = output<readonly RequestItem[]>();
  readonly statusChangeRequested = output<RequestItem>();
  readonly historyRequested = output<RequestItem>();

  protected readonly allSelected = computed(
    () => this.requests().length > 0 && this.requests().every((request) => this.selectedIds().has(request.id)),
  );
  protected readonly someSelected = computed(() => this.selectedIds().size > 0 && !this.allSelected());

  protected onSortChange(sort: Sort): void {
    const sortBy = requestSortFields.find((field) => field === sort.active);
    if (sortBy && sort.direction) {
      this.sortChange.emit({ sortBy, sortDirection: sort.direction === 'asc' ? 'Asc' : 'Desc' });
    }
  }

  protected toggleAll(): void {
    this.selectionChange.emit(this.allSelected() ? [] : this.requests());
  }

  protected toggle(request: RequestItem): void {
    const selected = this.requests().filter((item) =>
      item.id === request.id ? !this.selectedIds().has(item.id) : this.selectedIds().has(item.id),
    );
    this.selectionChange.emit(selected);
  }
}
