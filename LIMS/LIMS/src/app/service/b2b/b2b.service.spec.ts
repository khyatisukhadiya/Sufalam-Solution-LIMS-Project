import { TestBed } from '@angular/core/testing';

import { B2bService } from './b2b.service';

describe('B2bService', () => {
  let service: B2bService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(B2bService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
