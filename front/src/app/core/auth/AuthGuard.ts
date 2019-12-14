import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthenticationStateService } from '../services/AuthenticationStateService';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {
  constructor(private authenticationStateService: AuthenticationStateService, private router: Router) { }


  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): boolean {
    return this.checkIsLoggedIn(state.url, next.data.msg ? next.data.msg : '');
  }

  private checkIsLoggedIn(redirectUrl: string, message: string): boolean {
    console.log(redirectUrl);

    if (this.authenticationStateService.checkIsAuthenticated()) {
      return true;
    }
    this.router.navigate(['not-authenticated'], {state: {redirect: redirectUrl, msg: message}});

    return false;
  }
}
