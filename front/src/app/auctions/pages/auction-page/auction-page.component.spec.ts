import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AuctionPageComponent } from './auction-page.component';

describe('AuctionPageComponent', () => {
  let component: AuctionPageComponent;
  let fixture: ComponentFixture<AuctionPageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AuctionPageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AuctionPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
