import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BidCreatePageComponent } from './bid-create-page.component';

describe('BidCreatePageComponent', () => {
  let component: BidCreatePageComponent;
  let fixture: ComponentFixture<BidCreatePageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BidCreatePageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BidCreatePageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
