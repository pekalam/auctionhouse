import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ServerMessageService, ServerMessage } from '../services/ServerMessageService';
import { Observable, of } from 'rxjs';
import { filter, catchError, switchMap } from 'rxjs/operators';
import { CommandHelper } from './CommandHelper';

@Injectable({
  providedIn: 'root'
})
export class BidCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(auctionId: string, price: number): Observable<ServerMessage> {
    const url = '/api/bid';
    return this.commandHelper.getResponseStatusHandler(this.httpClient.post(url, { auctionId, price }), true);
  }
}
