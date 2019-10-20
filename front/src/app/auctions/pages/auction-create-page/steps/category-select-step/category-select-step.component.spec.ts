import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CategorySelectStepComponent } from './category-select-step.component';

describe('CategorySelectStepComponent', () => {
  let component: CategorySelectStepComponent;
  let fixture: ComponentFixture<CategorySelectStepComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CategorySelectStepComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CategorySelectStepComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
