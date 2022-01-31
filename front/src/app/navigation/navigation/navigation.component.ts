import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { AuthenticationStateService } from '../../core/services/AuthenticationStateService';
import { MatMenuModule, MatMenuTrigger } from '@angular/material';
import { TopAuctionsByTagQuery, TopAuctionsQueryResult, TopAuctionQueryItem } from '../../core/queries/TopAuctionsByTagQuery';
import { Subject, Observable } from 'rxjs';
import { distinctUntilChanged, debounceTime, switchMap, tap, map } from 'rxjs/operators';
import { Router } from '@angular/router';
import { LoadingService } from 'src/app/core/services/LoadingService';

@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.scss']
})
export class NavigationComponent implements OnInit {

  username: string;
  isAuthenticated = false;

  constructor(private router: Router, public authenticationStateService: AuthenticationStateService, private loadingService: LoadingService) {
    this.authenticationStateService.currentUser.subscribe((user) => {
      if (user) {
        this.username = user.userName;
      }
    })
    this.authenticationStateService.isAuthenticated.subscribe((isAuth) => this.isAuthenticated = isAuth);
  }

  ngOnInit() {
  }

  async onLogout() {
    this.loadingService.setLoading(true);
    await this.authenticationStateService.logout();
    this.loadingService.setLoading(false);
    this.router.navigateByUrl('home');
  }
}
