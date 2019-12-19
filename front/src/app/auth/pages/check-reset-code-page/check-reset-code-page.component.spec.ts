import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CheckResetCodePageComponent } from './check-reset-code-page.component';

describe('CheckResetCodePageComponent', () => {
  let component: CheckResetCodePageComponent;
  let fixture: ComponentFixture<CheckResetCodePageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CheckResetCodePageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CheckResetCodePageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
