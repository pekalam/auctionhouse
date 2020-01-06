import { HttpClient } from '@angular/common/http';
import { tap, map } from 'rxjs/operators';
import { UserIdentity } from '../../models/UserIdentity';
import * as jwtDecode from 'jwt-decode';
import { AuthenticationStateService } from '../../services/AuthenticationStateService';
import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SignInCommand {
  constructor(private httpClient: HttpClient, private authStateService: AuthenticationStateService) {
  }

  private parseUser(jwt: string): UserIdentity {
    const decoded = jwtDecode(jwt);
    return {
      userId: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid'],
      userName: decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']
    };
  }

  private storeWebUser(jwt: string) {
    if (!jwt) {
      throw new Error('null jwt');
    }
    localStorage.setItem('user', jwt);
  }

  execute(username: string, password: string) {
    return this.httpClient
      .post<string>(
        `${environment.API_URL}/api/signin`,
        { username, password }, { responseType: 'text' as 'json' }
      )
      .pipe(
        tap(user => {
          this.storeWebUser(user);
        }),
        map<string, UserIdentity>((v) => this.parseUser(v)),
        tap(user => {
          console.log(user);

          this.authStateService.notifyObservers(true, user);
        })
      );
  }


}
