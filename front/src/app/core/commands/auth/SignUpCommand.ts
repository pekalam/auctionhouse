import { HttpClient } from '@angular/common/http';
import { UserIdentity } from '../../models/UserIdentity';
import { Observable } from 'rxjs';
import * as jwtDecode from 'jwt-decode';
import { map, filter } from 'rxjs/operators';
import { WSCommandStatusService, RequestStatus } from '../../services/WSCommandStatusService';
import { Injectable } from '@angular/core';
import { HTTPCommandHelper } from '../HTTPCommandHelper';
import { CommandHelper, ResponseOptions } from '../ComandHelper';
import { environment } from '../../../../environments/environment';

export interface SignUpCommandArgs {
  username: string;
  password: string;
  email: string;
}

@Injectable({
  providedIn: 'root'
})
export class SignUpCommand {

  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(commandArgs: SignUpCommandArgs): Observable<RequestStatus> {
    const req = this.httpClient.post<RequestStatus>(`${environment.API_URL}/api/c/signup`, commandArgs);
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.HTTPQueuedCommand);
  }
}
