import { Observable, of } from 'rxjs';
import { ServerMessage, ServerMessageService } from '../services/ServerMessageService';
import { filter, catchError, switchMap } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';


@Injectable({
  providedIn: 'root'
})
export class StartAuctionCreateSessionCommand {

  constructor(private httpClient: HttpClient, private serverMessageService: ServerMessageService) {
  }

  execute() {
    console.log("start session command");

    const url = '/api/startCreateSession';
    const correlationId = '1234';
    return this.httpClient.post(url, {})
      .pipe(
        catchError((err) => {
          console.log(err);
          return of(err);
        }),
        switchMap((response: ServerMessage) => {
          console.log(response);
          if(response.status === "COMPLETED"){
            return of(response);
          }
          let handler = this.serverMessageService.setupServerMessageHandler(response.correlationId);
          return handler.pipe(filter((v) => v.correlationId === response.correlationId));
        })
      );
  }
}
