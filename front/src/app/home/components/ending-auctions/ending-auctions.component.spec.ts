import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EndingAuctionsComponent } from './ending-auctions.component';

describe('EndingAuctionsComponent', () => {
  let component: EndingAuctionsComponent;
  let fixture: ComponentFixture<EndingAuctionsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EndingAuctionsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EndingAuctionsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
