import { TestBed } from '@angular/core/testing';

import { TestapprovalService } from './testapproval.service';

describe('TestapprovalService', () => {
  let service: TestapprovalService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TestapprovalService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
