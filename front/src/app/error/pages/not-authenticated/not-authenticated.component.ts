import { Component, OnInit } from '@angular/core';
import { ActivatedRouteSnapshot, ActivatedRoute, RouterStateSnapshot, RouterState, Router } from '@angular/router';

@Component({
  selector: 'app-not-authenticated',
  templateUrl: './not-authenticated.component.html',
  styleUrls: ['./not-authenticated.component.scss']
})
export class NotAuthenticatedComponent implements OnInit {

  msg = "";

  constructor(private router: Router) {
    if (this.router.getCurrentNavigation().extras.state) {
      this.msg = this.router.getCurrentNavigation().extras.state.msg;

    }
  }

  ngOnInit() {
  }

}
