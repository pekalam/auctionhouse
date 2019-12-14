import { Observable, of } from 'rxjs';
import { ServerMessage, ServerMessageService } from '../services/ServerMessageService';

import { filter, catchError, switchMap } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CommandHelper } from './CommandHelper';


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

  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(args: UpdateAuctionCommandArgs): Observable<ServerMessage> {
    const url = '/api/userUpdateAuction';
    return this.commandHelper.getResponseStatusHandler(this.httpClient.post(url, { ...args }), true);
  }

}
