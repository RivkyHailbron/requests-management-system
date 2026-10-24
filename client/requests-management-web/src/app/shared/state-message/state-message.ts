import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

export type StateMessageKind = 'loading' | 'empty' | 'error';

@Component({
  selector: 'app-state-message',
  imports: [MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './state-message.html',
  styleUrl: './state-message.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StateMessage {
  readonly kind = input.required<StateMessageKind>();
  readonly message = input('');
  readonly retry = output();
}
