import { HttpClient } from '@angular/common/http';
import { tap, map } from 'rxjs/operators';
import { UserIdentity } from '../models/UserIdentity';
import * as jwtDecode from 'jwt-decode';
import { AuthenticationStateService } from '../services/AuthenticationStateService';

export class SignInCommand {
  constructor(private httpClient: HttpClient, private authStateService: AuthenticationStateService) {
  }

  private parseUser(jwt: string): UserIdentity {
    const decoded = jwtDecode(jwt);
    return {
      userId: decoded.sid,
      userName: decoded.name
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
        '/api/signin',
        { username, password }, { responseType: 'text' as 'json' }
      )
      .pipe(
        tap(user => {
          this.storeWebUser(user);
        }),
        map<string, UserIdentity>((v) => this.parseUser(v)),
        tap(user => {
          this.authStateService.notifyObservers(true, user);
        })
      );
  }


}
