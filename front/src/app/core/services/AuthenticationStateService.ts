import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { distinctUntilChanged } from 'rxjs/operators';
import { UserIdentity } from '../models/UserIdentity';
import * as jwtDecode from 'jwt-decode';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationStateService {
  protected isAuthenticatedSubject = new ReplaySubject<boolean>(0);
  public isAuthenticated = this.isAuthenticatedSubject.asObservable().pipe(distinctUntilChanged());

  protected currentUserSubject = new ReplaySubject<UserIdentity>(0);
  public currentUser = this.currentUserSubject.asObservable().pipe(distinctUntilChanged());

  constructor(private router: Router) {
    this.isAuthenticatedSubject.next(false);
    this.currentUserSubject.next(null);
  }

  private getJwtFromStorage(): string {
    const userStr = localStorage.getItem('user');
    if (!userStr) {
      throw new Error('Cannot find user in localStorage');
    }
    return userStr;
  }

  private parseUser(jwt: string): UserIdentity {
    const decoded = jwtDecode(jwt);
    return {
      userId: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid'],
      userName: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']
    };
  }

  notifyObservers(isAuthenticated: boolean, currentUser: UserIdentity) {
    console.log("NOTF");

    this.isAuthenticatedSubject.next(isAuthenticated);
    this.currentUserSubject.next(currentUser);
  }

  getAuthorizationHttpHeader(): string | null {
    try {
      const jwt = this.getJwtFromStorage();
      return `Bearer ${jwt}`;
    } catch (error) {
      return null;
    }
  }

  checkIsAuthenticated(): boolean {
    try {
      const jwt = this.getJwtFromStorage();
      const userIdentity = this.parseUser(jwt);
      this.notifyObservers(true, userIdentity);
      return true;
    } catch (error) {
      this.notifyObservers(false, null);
      return false;
    }
  }

  logout() {
    localStorage.removeItem('user');
    this.notifyObservers(false, null);
    this.router.navigateByUrl('home');
  }
}
