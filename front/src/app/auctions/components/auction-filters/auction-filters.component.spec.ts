import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AuctionFiltersComponent } from './auction-filters.component';

describe('AuctionFiltersComponent', () => {
  let component: AuctionFiltersComponent;
  let fixture: ComponentFixture<AuctionFiltersComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AuctionFiltersComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AuctionFiltersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
