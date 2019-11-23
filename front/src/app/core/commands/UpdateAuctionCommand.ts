import { Observable, of } from 'rxjs';
import { ServerMessage, ServerMessageService } from '../services/ServerMessageService';

import { filter, catchError, switchMap } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';


export interface UpdateAuctionCommandArgs {
  auctionId: string;
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

  constructor(private httpClient: HttpClient, private serverMessageService: ServerMessageService) {
  }

  execute(args: UpdateAuctionCommandArgs): Observable<ServerMessage> {
    const url = '/api/userUpdateAuction';
    return this.httpClient.post(url, { ...args })
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
