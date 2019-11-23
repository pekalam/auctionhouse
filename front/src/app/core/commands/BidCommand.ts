import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ServerMessageService, ServerMessage } from '../services/ServerMessageService';
import { Observable, of } from 'rxjs';
import { filter, catchError, switchMap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class BidCommand {
  constructor(private httpClient: HttpClient, private serverMessageService: ServerMessageService) {
  }

  execute(auctionId: string, price: number): Observable<ServerMessage> {
    const url = '/api/bid';
    return this.httpClient.post(url, { auctionId, price })
      .pipe(
        catchError((err) => {
          console.log(err);
          return of(err);
        }),
        switchMap((response: ServerMessage) => {
          console.log(response);
          if (response.status === "COMPLETED") {
            return of(response);
          }
          let handler = this.serverMessageService.setupServerMessageHandler(response.correlationId);
          return handler.pipe(filter((v) => v.correlationId === response.correlationId));
        })
      );
  }
}
