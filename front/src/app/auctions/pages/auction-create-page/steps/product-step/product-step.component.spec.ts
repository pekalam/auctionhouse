import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductStepComponent } from './product-step.component';

describe('ProductStepComponent', () => {
  let component: ProductStepComponent;
  let fixture: ComponentFixture<ProductStepComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ProductStepComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ProductStepComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
