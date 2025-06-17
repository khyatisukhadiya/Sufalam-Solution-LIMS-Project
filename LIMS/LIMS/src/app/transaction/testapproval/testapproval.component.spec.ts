import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TestapprovalComponent } from './testapproval.component';

describe('TestapprovalComponent', () => {
  let component: TestapprovalComponent;
  let fixture: ComponentFixture<TestapprovalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestapprovalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TestapprovalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
