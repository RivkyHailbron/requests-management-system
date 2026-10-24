import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/requests/requests-page/requests-page').then((module) => module.RequestsPage),
  },
  { path: '**', redirectTo: '' },
];
