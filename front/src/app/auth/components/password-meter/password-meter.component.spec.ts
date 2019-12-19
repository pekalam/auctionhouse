import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PasswordMeterComponent } from './password-meter.component';

describe('PasswordMeterComponent', () => {
  let component: PasswordMeterComponent;
  let fixture: ComponentFixture<PasswordMeterComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PasswordMeterComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PasswordMeterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
