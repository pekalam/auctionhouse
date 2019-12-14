import { HttpClient } from '@angular/common/http';
import { UserIdentity } from '../models/UserIdentity';
import { Observable } from 'rxjs';
import * as jwtDecode from 'jwt-decode';
import { map, filter } from 'rxjs/operators';
import { ServerMessageService, ServerMessage } from '../services/ServerMessageService';
import { Injectable } from '@angular/core';
import { CommandHelper } from './CommandHelper';

@Injectable({
  providedIn: 'root'
})
export class ChangePasswordCommand {

  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(password: string): Observable<any> {
    return this.commandHelper.getResponseStatusHandler(this.httpClient.post(
      '/api/changePassword', { newPassword: password }
    ), true);
  }
}
