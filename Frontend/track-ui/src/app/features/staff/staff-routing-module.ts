import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StaffTickets } from './tickets/tickets';
import { StaffPolicy } from './policy/policy';

const routes: Routes = [
  { path: '', redirectTo: 'tickets', pathMatch: 'full' },
  { path: 'tickets', component: StaffTickets },
  { path: 'policy', component: StaffPolicy }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StaffRoutingModule {}