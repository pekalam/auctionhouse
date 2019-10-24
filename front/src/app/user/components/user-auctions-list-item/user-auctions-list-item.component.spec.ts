import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UserAuctionsListItemComponent } from './user-auctions-list-item.component';

describe('UserAuctionsListItemComponent', () => {
  let component: UserAuctionsListItemComponent;
  let fixture: ComponentFixture<UserAuctionsListItemComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ UserAuctionsListItemComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserAuctionsListItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
