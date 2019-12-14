import { Component, OnInit } from '@angular/core';
import { AuthenticationStateService } from './core/services/AuthenticationStateService';
import { LoadingService } from './core/services/LoadingService';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {

  constructor(private authenticationStateService: AuthenticationStateService, public loadingService: LoadingService) { }

  title = 'front';

  ngOnInit(): void {
    this.authenticationStateService.checkIsAuthenticated();
  }
}
