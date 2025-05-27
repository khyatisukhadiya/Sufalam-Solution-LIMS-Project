import { TestBed } from '@angular/core/testing';

import { SampleRegisterService } from './sample-register.service';

describe('SampleRegisterService', () => {
  let service: SampleRegisterService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SampleRegisterService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
