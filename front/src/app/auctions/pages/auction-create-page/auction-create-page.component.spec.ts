import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AuctionCreatePageComponent } from './auction-create-page.component';

describe('AuctionCreatePageComponent', () => {
  let component: AuctionCreatePageComponent;
  let fixture: ComponentFixture<AuctionCreatePageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AuctionCreatePageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AuctionCreatePageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
