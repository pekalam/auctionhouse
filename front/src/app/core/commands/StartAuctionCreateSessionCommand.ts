import { Observable, of } from 'rxjs';
import { ServerMessage, ServerMessageService } from '../services/ServerMessageService';
import { filter, catchError, switchMap } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CommandHelper } from './CommandHelper';


@Injectable({
  providedIn: 'root'
})
export class StartAuctionCreateSessionCommand {

  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute() {
    console.log("start session command");

    const url = '/api/startCreateSession';
    return this.commandHelper.getResponseStatusHandler(this.httpClient.post(url, {}), true);
  }
}
