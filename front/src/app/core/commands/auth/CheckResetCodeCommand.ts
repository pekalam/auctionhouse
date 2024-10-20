import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RequestStatus } from '../../services/WSCommandStatusService';
import { CommandHelper, ResponseOptions } from '../ComandHelper';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CheckResetCodeCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(resetCode: string, email: string): Observable<RequestStatus> {
    const url = `${environment.API_URL}/api/c/checkResetCode`;
    const req = this.httpClient.post(url, { resetCode, email });
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.HTTPQueuedCommand);
  }
}
