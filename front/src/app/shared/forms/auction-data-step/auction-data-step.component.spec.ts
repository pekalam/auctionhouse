import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AuctionDataStepComponent } from './auction-data-step.component';

describe('AuctionDataStepComponent', () => {
  let component: AuctionDataStepComponent;
  let fixture: ComponentFixture<AuctionDataStepComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AuctionDataStepComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AuctionDataStepComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
