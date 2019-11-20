import { Observable } from 'rxjs';
import { ServerMessage, ServerMessageService } from '../services/ServerMessageService';

import { filter } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';


export interface UpdateAuctionCommandArgs {
  auctionId: string;
  correlationId: string;
  buyNowPrice: number | null;
  endDate: Date | null;
  category: string[];
  description: string;
  tags: string[];
  name: string;
}

@Injectable({
  providedIn: 'root'
})
export class UpdateAuctionCommand {
  private handler: Observable<ServerMessage>;

  constructor(private httpClient: HttpClient, private serverMessageService: ServerMessageService) {
    this.handler = serverMessageService.setupServerMessageHandler('auctionUpdated');
  }

  execute(args: UpdateAuctionCommandArgs): Observable<ServerMessage> {
    const url = '/api/userUpdateAuction';
    this.httpClient.post(url, { ...args }).subscribe((o) => {
      console.log(o);
    });
    return this.handler.pipe(filter((v) => v.correlationId === args.correlationId));
  }

}
