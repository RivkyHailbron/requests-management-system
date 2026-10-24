import { Injectable, inject, signal } from '@angular/core';
import { APP_CONFIG } from '../config/app-config';

@Injectable({ providedIn: 'root' })
export class CurrentUser {
  private readonly userId = signal(inject(APP_CONFIG).defaultUserId);

  readonly id = this.userId.asReadonly();

  change(userId: string): void {
    const trimmed = userId.trim();
    if (trimmed) {
      this.userId.set(trimmed);
    }
  }
}
