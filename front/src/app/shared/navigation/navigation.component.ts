import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { AuthenticationStateService } from '../../core/services/AuthenticationStateService';
import { MatMenuModule, MatMenuTrigger } from '@angular/material';
import { TopAuctionsByTagQuery, TopAuctionsQueryResult, TopAuctionQueryItem } from '../../core/queries/AuctionsByTagQuery';
import { Subject, Observable } from 'rxjs';
import { distinctUntilChanged, debounceTime, switchMap, tap, map } from 'rxjs/operators';

@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.scss']
})
export class NavigationComponent implements OnInit {

  username: string;
  isAuthenticated = false;

  constructor(public authenticationStateService: AuthenticationStateService, private tagsQuery: TopAuctionsByTagQuery) {
    this.authenticationStateService.currentUser.subscribe((user) => {
      if (user) {
        this.username = user.userName;
      }
    })
    this.authenticationStateService.isAuthenticated.subscribe((isAuth) => this.isAuthenticated = isAuth);
  }

  ngOnInit() {
  }

  onLogout() {
    this.authenticationStateService.logout();
  }
}
