import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { roleGuard } from './core/guards/role-guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login').then(m => m.Login)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register').then(m => m.Register)
  },
  {
    path: 'customer',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Customer'] },
    loadChildren: () =>
      import('./features/customer/customer-module').then(m => m.CustomerModule)
  },
  {
    path: 'staff',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['SupportAgent'] },
    loadChildren: () =>
      import('./features/staff/staff-module').then(m => m.StaffModule)
  },
  {
    path: 'admin',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] },
    loadChildren: () =>
      import('./features/admin/admin-module').then(m => m.AdminModule)
  },
  { path: '**', redirectTo: 'login' }
];