import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UserAuctionsPageComponent } from './user-auctions-page.component';

describe('UserAuctionsPageComponent', () => {
  let component: UserAuctionsPageComponent;
  let fixture: ComponentFixture<UserAuctionsPageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ UserAuctionsPageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserAuctionsPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
