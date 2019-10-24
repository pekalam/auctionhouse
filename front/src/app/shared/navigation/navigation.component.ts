import { Component, OnInit, ViewChild } from '@angular/core';
import { AuthenticationStateService } from '../../core/services/AuthenticationStateService';
import { MatMenuModule, MatMenuTrigger } from '@angular/material';

@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.scss']
})
export class NavigationComponent implements OnInit {

  username: string;
  isAuthenticated = false;

  constructor(public authenticationStateService: AuthenticationStateService) {
    console.log("ctor");

    this.authenticationStateService.currentUser.subscribe((user) => {
      console.log("nav sub " + user);

      if (user) {
      console.log("nav sub " + user.userName);

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
