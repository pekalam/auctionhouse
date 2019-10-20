import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddImageStepComponent } from './add-image-step.component';

describe('AddImageStepComponent', () => {
  let component: AddImageStepComponent;
  let fixture: ComponentFixture<AddImageStepComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddImageStepComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddImageStepComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
