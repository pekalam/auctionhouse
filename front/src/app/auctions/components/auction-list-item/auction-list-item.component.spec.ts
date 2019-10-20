import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AuctionListItemComponent } from './auction-list-item.component';

describe('AuctionListItemComponent', () => {
  let component: AuctionListItemComponent;
  let fixture: ComponentFixture<AuctionListItemComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AuctionListItemComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AuctionListItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
