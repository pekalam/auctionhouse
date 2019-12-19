import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ServerMessage } from '../services/ServerMessageService';
import { CommandHelper } from './CommandHelper';

@Injectable({
  providedIn: 'root'
})
export class CheckResetCodeCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(resetCode: string, email: string): Observable<ServerMessage> {
    const url = '/api/checkResetCode';
    return this.commandHelper.getResponseStatusHandler(this.httpClient.post(url, { resetCode, email }), true);
  }
}
