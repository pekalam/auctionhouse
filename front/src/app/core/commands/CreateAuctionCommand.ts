import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { ServerMessage, ServerMessageService } from '../services/ServerMessageService';
import { filter, catchError, switchMap } from 'rxjs/operators';
import { Product } from '../models/Product';

export class CreateAuctionCommandArgs {
  constructor(public buyNowPrice: number | null, public startDate: Date, public endDate: Date, public category: Array<string>
    , public product: Product, public tags: string[], public name: string, public buyNowOnly: boolean) {

  }
}

@Injectable({
  providedIn: 'root'
})
export class CreateAuctionCommand {
  constructor(private httpClient: HttpClient, private serverMessageService: ServerMessageService) {
  }

  execute(commandArgs: CreateAuctionCommandArgs): Observable<ServerMessage> {
    const url = '/api/createAuction';
    console.log(commandArgs);

    return this.httpClient.post(url, commandArgs)
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
