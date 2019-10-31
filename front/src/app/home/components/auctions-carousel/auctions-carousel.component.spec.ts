import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AuctionsCarouselComponent } from './auctions-carousel.component';

describe('AuctionsCarouselComponent', () => {
  let component: AuctionsCarouselComponent;
  let fixture: ComponentFixture<AuctionsCarouselComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AuctionsCarouselComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AuctionsCarouselComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
