import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AuctionsPageComponent } from './auctions-page.component';

describe('AuctionsPageComponent', () => {
  let component: AuctionsPageComponent;
  let fixture: ComponentFixture<AuctionsPageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AuctionsPageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AuctionsPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
