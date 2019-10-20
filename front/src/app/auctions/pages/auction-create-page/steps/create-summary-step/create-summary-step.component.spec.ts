import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateSummaryStepComponent } from './create-summary-step.component';

describe('CreateSummaryStepComponent', () => {
  let component: CreateSummaryStepComponent;
  let fixture: ComponentFixture<CreateSummaryStepComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreateSummaryStepComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateSummaryStepComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
