import { HttpClient } from '@angular/common/http';
import { UserIdentity } from '../models/UserIdentity';
import { Observable } from 'rxjs';
import * as jwtDecode from 'jwt-decode';
import { map, filter } from 'rxjs/operators';
import { ServerMessageService, ServerMessage } from '../services/ServerMessageService';
import { Injectable } from '@angular/core';

export class SignUpCommandArgs {
  constructor(public username: string, public password: string, public correlationId: string) {

  }
}

@Injectable({
  providedIn: 'root'
})
export class SignUpCommand {

  constructor(private httpClient: HttpClient) {
  }

  execute(commandArgs: SignUpCommandArgs): Observable<any> {
    return this.httpClient.post(
      '/api/signup',
      commandArgs
    );
  }
}
