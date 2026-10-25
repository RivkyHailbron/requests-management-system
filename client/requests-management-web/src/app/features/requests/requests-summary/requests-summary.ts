import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { DecimalPipe, PercentPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { RequestsSummary as Summary, requestStatusLabels } from '../models/request.models';

interface SummaryRow {
  readonly key: string;
  readonly label: string;
  readonly count: number;
  readonly share: number;
}

@Component({
  selector: 'app-requests-summary',
  imports: [DecimalPipe, PercentPipe, MatCardModule, MatIconModule],
  templateUrl: './requests-summary.html',
  styleUrl: './requests-summary.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RequestsSummary {
  readonly summary = input.required<Summary>();

  protected readonly statusRows = computed(() =>
    this.summary().byStatus.map(({ status, count }) => this.toRow(status, requestStatusLabels[status], count)),
  );

  protected readonly priorityRows = computed(() =>
    this.summary().byPriority.map(({ priority, count }) => this.toRow(priority, priority, count)),
  );

  private toRow(key: string, label: string, count: number): SummaryRow {
    const total = this.summary().totalCount;
    return { key, label, count, share: total > 0 ? count / total : 0 };
  }
}
