import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AuctionEditPageComponent } from './auction-edit-page.component';

describe('AuctionEditPageComponent', () => {
  let component: AuctionEditPageComponent;
  let fixture: ComponentFixture<AuctionEditPageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AuctionEditPageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AuctionEditPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
