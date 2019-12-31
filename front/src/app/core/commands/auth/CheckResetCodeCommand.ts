import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RequestStatus } from '../../services/WSCommandStatusService';
import { CommandHelper, ResponseOptions } from '../ComandHelper';

@Injectable({
  providedIn: 'root'
})
export class CheckResetCodeCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(resetCode: string, email: string): Observable<RequestStatus> {
    const url = '/api/checkResetCode';
    const req = this.httpClient.post(url, { resetCode, email });
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.HTTPQueuedCommand);
  }
}
