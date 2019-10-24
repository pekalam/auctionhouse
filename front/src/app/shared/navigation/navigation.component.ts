import { Component, OnInit, ViewChild } from '@angular/core';
import { AuthenticationStateService } from '../../core/services/AuthenticationStateService';
import { MatMenuModule, MatMenuTrigger } from '@angular/material';

@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.scss']
})
export class NavigationComponent implements OnInit {

  constructor(public authenticationStateService: AuthenticationStateService) {}

  ngOnInit() {
    this.authenticationStateService.checkIsAuthenticated();
  }

  onLogout(){
    this.authenticationStateService.logout();
  }
}
