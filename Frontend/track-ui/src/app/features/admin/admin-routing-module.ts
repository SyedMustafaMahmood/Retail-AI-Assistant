import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminTickets } from './tickets/tickets';
import { AdminPolicy } from './policy/policy';

const routes: Routes = [
  { path: '', redirectTo: 'tickets', pathMatch: 'full' },
  { path: 'tickets', component: AdminTickets },
  { path: 'policy', component: AdminPolicy }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule {}