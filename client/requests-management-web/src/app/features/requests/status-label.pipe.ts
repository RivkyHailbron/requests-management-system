import { Pipe, PipeTransform } from '@angular/core';
import { RequestStatus, requestStatusLabels } from './models/request.models';

@Pipe({ name: 'statusLabel' })
export class StatusLabelPipe implements PipeTransform {
  transform(status: RequestStatus): string {
    return requestStatusLabels[status];
  }
}
