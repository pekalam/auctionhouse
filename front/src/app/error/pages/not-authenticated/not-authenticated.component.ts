import { Component, OnInit } from '@angular/core';
import { ActivatedRouteSnapshot, ActivatedRoute, RouterStateSnapshot, RouterState, Router } from '@angular/router';

@Component({
  selector: 'app-not-authenticated',
  templateUrl: './not-authenticated.component.html',
  styleUrls: ['./not-authenticated.component.scss']
})
export class NotAuthenticatedComponent implements OnInit {

  msg = "";
  redirect = null;

  constructor(private router: Router) {
    if (this.router.getCurrentNavigation().extras.state) {
      this.msg = this.router.getCurrentNavigation().extras.state.msg;
      this.redirect = this.router.getCurrentNavigation().extras.state.redirect;
    }
  }

  ngOnInit() {
  }

  onSignInClick() {
    if (this.redirect) {
      this.router.navigate(['/sign-in'], { state: { redirect: this.redirect } });
    } else {
      this.router.navigate(['/sign-in']);
    }
  }

}
