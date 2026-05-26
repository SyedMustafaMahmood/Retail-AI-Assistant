import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminTickets } from './tickets';

describe('Tickets', () => {
  let component: AdminTickets;
  let fixture: ComponentFixture<AdminTickets>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminTickets],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminTickets);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
