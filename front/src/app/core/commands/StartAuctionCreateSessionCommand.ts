import { Observable } from 'rxjs';
import { ServerMessage, ServerMessageService } from '../services/ServerMessageService';
import { filter } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

export class StartAuctionCreateSessionCommandArgs {
  constructor(public correlationId: string) { }
}

@Injectable({
  providedIn: 'root'
})
export class StartAuctionCreateSessionCommand {
  private handler: Observable<ServerMessage>;

  constructor(private httpClient: HttpClient, private serverMessageService: ServerMessageService) {
    this.handler = serverMessageService.setupServerMessageHandler('auctionCreateSessionStarted');
  }

  execute() {
    console.log("start session command");

    const url = '/api/startCreateSession';
    const correlationId = '1234';
    let msgHandler = this.handler.pipe(filter((v) => v.correlationId === correlationId));
    this.httpClient.post(url, new StartAuctionCreateSessionCommandArgs(correlationId)).subscribe((o) => {
      console.log(o);
    }, (err) => {
      console.log(err);

    });
    return msgHandler;
  }
}
