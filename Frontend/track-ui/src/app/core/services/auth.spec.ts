import { TestBed } from '@angular/core/testing';

import { Auth } from './auth';
import { zip } from 'rxjs';

describe('Auth', () => {
  let service: Auth;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Auth);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});