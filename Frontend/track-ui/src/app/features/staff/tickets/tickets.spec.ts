import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StaffTickets } from './tickets';

describe('Tickets', () => {
  let component: StaffTickets;
  let fixture: ComponentFixture<StaffTickets>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StaffTickets],
    }).compileComponents();

    fixture = TestBed.createComponent(StaffTickets);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
