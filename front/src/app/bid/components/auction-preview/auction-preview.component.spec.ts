import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AuctionPreviewComponent } from './auction-preview.component';

describe('AuctionPreviewComponent', () => {
  let component: AuctionPreviewComponent;
  let fixture: ComponentFixture<AuctionPreviewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AuctionPreviewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AuctionPreviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
