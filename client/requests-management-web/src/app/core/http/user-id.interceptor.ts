import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { APP_CONFIG } from '../config/app-config';
import { CurrentUser } from '../user/current-user';

export const userIdInterceptor: HttpInterceptorFn = (request, next) => {
  const config = inject(APP_CONFIG);
  const currentUser = inject(CurrentUser);

  return next(request.clone({ setHeaders: { [config.userIdHeader]: currentUser.id() } }));
};
