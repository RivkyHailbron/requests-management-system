import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { catchError, map, of, startWith } from 'rxjs';
import { toErrorMessage } from '../../../core/http/problem-details';
import { StateMessage } from '../../../shared/state-message/state-message';
import { RequestItem, StatusHistoryEntry } from '../models/request.models';
import { RequestsStore } from '../state/requests-store';

type HistoryState =
  | { readonly kind: 'loading' }
  | { readonly kind: 'error'; readonly message: string }
  | { readonly kind: 'loaded'; readonly entries: readonly StatusHistoryEntry[] };

@Component({
  selector: 'app-request-history-dialog',
  imports: [DatePipe, MatButtonModule, MatDialogModule, StateMessage],
  templateUrl: './request-history-dialog.html',
  styleUrl: './request-history-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RequestHistoryDialog {
  protected readonly request = inject<RequestItem>(MAT_DIALOG_DATA);

  protected readonly state = toSignal(
    inject(RequestsStore)
      .history(this.request.id)
      .pipe(
        map((entries): HistoryState => ({ kind: 'loaded', entries })),
        catchError((error: unknown) => of<HistoryState>({ kind: 'error', message: toErrorMessage(error) })),
        startWith<HistoryState>({ kind: 'loading' }),
      ),
    { requireSync: true },
  );
}
