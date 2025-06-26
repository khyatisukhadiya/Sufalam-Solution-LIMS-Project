import { TestBed } from '@angular/core/testing';

import { UserregistrationService } from './userregistration.service';

describe('UserregistrationService', () => {
  let service: UserregistrationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UserregistrationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
