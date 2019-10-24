import { Component, OnInit } from '@angular/core';
import { AuthenticationStateService } from './core/services/AuthenticationStateService';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {

  constructor(private authenticationStateService: AuthenticationStateService) { }

  title = 'front';

  ngOnInit(): void {
    this.authenticationStateService.checkIsAuthenticated();
  }
}
