import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { ServerMessage, ServerMessageService } from '../services/ServerMessageService';
import { filter, catchError, switchMap } from 'rxjs/operators';
import { Product } from '../models/Product';
import { CommandHelper } from './CommandHelper';

@Injectable({
  providedIn: 'root'
})
export class ResetPasswordCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(resetCode: string, newPassword: string, email: string): Observable<ServerMessage>{
    const url = '/api/resetPassword';
    return this.commandHelper.getResponseStatusHandler(this.httpClient.post(url, {resetCode, newPassword, email}), true);
  }
}
