import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { NotAuthenticatedComponent } from './not-authenticated.component';

describe('NotAuthenticatedComponent', () => {
  let component: NotAuthenticatedComponent;
  let fixture: ComponentFixture<NotAuthenticatedComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ NotAuthenticatedComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(NotAuthenticatedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
