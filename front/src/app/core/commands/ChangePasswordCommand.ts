import { HttpClient } from '@angular/common/http';
import { UserIdentity } from '../models/UserIdentity';
import { Observable } from 'rxjs';
import * as jwtDecode from 'jwt-decode';
import { map, filter } from 'rxjs/operators';
import { ServerMessageService, ServerMessage } from '../services/ServerMessageService';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ChangePasswordCommand {

  constructor(private httpClient: HttpClient) {
  }

  execute(password: string): Observable<any> {
    return this.httpClient.post(
      '/api/changePassword', { newPassword: password }
    );
  }
}
