import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { RequestStatus, WSCommandStatusService } from '../../services/WSCommandStatusService';
import { CommandHelper, ResponseOptions } from '../ComandHelper';

@Injectable({
  providedIn: 'root'
})
export class ResetPasswordCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(resetCode: string, newPassword: string, email: string): Observable<RequestStatus>{
    const url = '/api/resetPassword';
    const req = this.httpClient.post(url, {resetCode, newPassword, email});
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.HTTPQueuedCommand);
  }
}
