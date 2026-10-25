import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { LookupAutocomplete } from '../../../shared/lookup-autocomplete/lookup-autocomplete';
import {
  RequestFilters,
  RequestPriority,
  RequestStatus,
  emptyFilters,
  requestPriorities,
  requestStatuses,
} from '../models/request.models';

@Component({
  selector: 'app-requests-filters',
  imports: [
    ReactiveFormsModule,
    LookupAutocomplete,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
  ],
  templateUrl: './requests-filters.html',
  styleUrl: './requests-filters.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RequestsFilters {
  protected readonly statuses = requestStatuses;
  protected readonly priorities = requestPriorities;

  protected readonly form = inject(NonNullableFormBuilder).group({
    search: emptyFilters.search,
    statuses: [[] as RequestStatus[]],
    priorities: [[] as RequestPriority[]],
    organizationName: emptyFilters.organizationName,
    assignedTo: emptyFilters.assignedTo,
    createdFrom: emptyFilters.createdFrom,
    createdTo: emptyFilters.createdTo,
  });

  readonly organizationOptions = input.required<readonly string[]>();
  readonly assigneeOptions = input.required<readonly string[]>();

  readonly filtersChange = output<RequestFilters>();
  readonly organizationSearch = output<string>();
  readonly assigneeSearch = output<string>();

  constructor() {
    this.form.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.filtersChange.emit(this.form.getRawValue()));
  }

  protected clear(): void {
    this.form.reset();
  }
}
