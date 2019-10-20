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
    return this.checkIsLoggedIn(state.url);
  }

  private checkIsLoggedIn(redirectUrl: string): boolean {
    if (this.authenticationStateService.checkIsAuthenticated()) {
      return true;
    }
    this.router.navigate(['sign-in'], {state: {redirect: redirectUrl}});

    return false;
  }
}
