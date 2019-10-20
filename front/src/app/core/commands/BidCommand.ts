import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ServerMessageService, ServerMessage } from '../services/ServerMessageService';
import { Observable } from 'rxjs';
import { filter } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class BidCommand {
  private handler: Observable<ServerMessage>;

  constructor(private httpClient: HttpClient, private serverMessageService: ServerMessageService) {
    this.handler = serverMessageService.setupServerMessageHandler('auctionRaised');
  }

  execute(auctionId: string, price: number,
          correlationId: string): Observable<ServerMessage> {
    const url = '/api/bid';
    this.httpClient.post(url, { auctionId, price, correlationId }).subscribe((o) => {
      console.log(o);
    });
    return this.handler.pipe(filter((v) => v.correlationId === correlationId));
  }
}
