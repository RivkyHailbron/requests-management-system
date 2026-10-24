import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { RequestsSummary as Summary } from '../models/request.models';

@Component({
  selector: 'app-requests-summary',
  imports: [DecimalPipe, MatCardModule],
  templateUrl: './requests-summary.html',
  styleUrl: './requests-summary.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RequestsSummary {
  readonly summary = input.required<Summary>();
}
