import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { RequestStatus, WSCommandStatusService } from '../../services/WSCommandStatusService';
import { ResponseOptions, CommandHelper } from '../ComandHelper';

@Injectable({
  providedIn: 'root'
})
export class RequestResetPasswordCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(email: string): Observable<RequestStatus>{
    const url = '/api/requestResetPassword';
    const req = this.httpClient.post(url, {email});
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.HTTPQueuedCommand);
  }
}
