import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { RequestStatus, requestStatuses } from '../models/request.models';

@Component({
  selector: 'app-bulk-status-bar',
  imports: [ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatSelectModule],
  templateUrl: './bulk-status-bar.html',
  styleUrl: './bulk-status-bar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BulkStatusBar {
  protected readonly statuses = requestStatuses;
  protected readonly statusControl = new FormControl<RequestStatus | null>(null, Validators.required);

  readonly selectedCount = input.required<number>();
  readonly maxItems = input.required<number>();
  readonly busy = input(false);

  readonly apply = output<RequestStatus>();
  readonly clearSelection = output();

  protected submit(): void {
    const status = this.statusControl.value;
    if (status) {
      this.apply.emit(status);
    }
  }
}
