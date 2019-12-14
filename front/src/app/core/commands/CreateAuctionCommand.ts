import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { ServerMessage, ServerMessageService } from '../services/ServerMessageService';
import { filter, catchError, switchMap } from 'rxjs/operators';
import { Product } from '../models/Product';
import { CommandHelper } from './CommandHelper';

export interface CreateAuctionCommandArgs {
  buyNowPrice: number | null;
  startDate: Date;
  endDate: Date;
  category: Array<string>;
  product: Product;
  tags: string[];
  name: string;
  buyNowOnly: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class CreateAuctionCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(commandArgs: CreateAuctionCommandArgs): Observable<ServerMessage> {
    const url = '/api/createAuction';
    console.log(commandArgs);

    return this.commandHelper.getResponseStatusHandler(this.httpClient.post(url, commandArgs), true);
  }
}
