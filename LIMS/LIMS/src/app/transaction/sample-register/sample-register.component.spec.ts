import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SampleRegisterComponent } from './sample-register.component';

describe('SampleRegisterComponent', () => {
  let component: SampleRegisterComponent;
  let fixture: ComponentFixture<SampleRegisterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SampleRegisterComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SampleRegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
