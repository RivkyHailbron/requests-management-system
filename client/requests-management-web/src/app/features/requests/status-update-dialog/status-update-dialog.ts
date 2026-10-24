import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { toErrorMessage, toProblemDetails } from '../../../core/http/problem-details';
import { RequestItem, RequestStatus } from '../models/request.models';
import { RequestsStore } from '../state/requests-store';

const currentRequestExtension = 'currentRequest';

@Component({
  selector: 'app-status-update-dialog',
  imports: [ReactiveFormsModule, MatButtonModule, MatDialogModule, MatFormFieldModule, MatProgressBarModule, MatSelectModule],
  templateUrl: './status-update-dialog.html',
  styleUrl: './status-update-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusUpdateDialog {
  private readonly store = inject(RequestsStore);
  private readonly dialogRef = inject<MatDialogRef<StatusUpdateDialog, RequestItem>>(MatDialogRef);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly request = signal(inject<RequestItem>(MAT_DIALOG_DATA));
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly conflictMessage = signal<string | null>(null);
  protected readonly allowedStatuses = computed(() => this.store.allowedTargets(this.request().status));
  protected readonly statusControl = new FormControl<RequestStatus | null>(null, Validators.required);

  protected save(): void {
    const status = this.statusControl.value;
    if (!status || this.saving()) {
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);
    this.conflictMessage.set(null);

    this.store
      .updateStatus(this.request(), status)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => this.dialogRef.close(updated),
        error: (error: unknown) => this.handleError(error),
      });
  }

  private handleError(error: unknown): void {
    this.saving.set(false);

    const currentRequest = toProblemDetails(error)?.[currentRequestExtension] as RequestItem | undefined;
    if (error instanceof HttpErrorResponse && error.status === HttpStatusCode.Conflict && currentRequest) {
      this.request.set(currentRequest);
      this.statusControl.reset();
      this.conflictMessage.set(
        `This request was updated by another user and is now '${currentRequest.status}'. ` +
          'The form shows the latest version; choose a status again to continue.',
      );
      return;
    }

    this.errorMessage.set(toErrorMessage(error));
  }
}
