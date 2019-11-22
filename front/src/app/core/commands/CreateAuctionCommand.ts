import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ServerMessage, ServerMessageService } from '../services/ServerMessageService';
import { filter } from 'rxjs/operators';
import { Product } from '../models/Product';

export class CreateAuctionCommandArgs {
  constructor(public buyNowPrice: number | null, public startDate: Date, public endDate: Date, public category: Array<string>
              , public correlationId: string, public product: Product, public tags: string[], public name: string, public buyNowOnly: boolean) {

  }
}

@Injectable({
  providedIn: 'root'
})
export class CreateAuctionCommand {
  private handler: Observable<ServerMessage>;

  constructor(private httpClient: HttpClient, private serverMessageService: ServerMessageService) {
    this.handler = serverMessageService.setupServerMessageHandler('auctionCreated');
  }

  execute(commandArgs: CreateAuctionCommandArgs): Observable<ServerMessage> {
    const url = '/api/createAuction';
    console.log(commandArgs);

    this.httpClient.post(url, commandArgs).subscribe((o) => {
      console.log(o);
    });
    return this.handler.pipe(filter((v) => v.correlationId === commandArgs.correlationId));
  }
}
