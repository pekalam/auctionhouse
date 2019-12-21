import { Observable, of } from 'rxjs';
import { RequestStatus, WSCommandStatusService } from '../services/WSCommandStatusService';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CommandHelper, ResponseOptions } from './ComandHelper';


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

  execute(args: UpdateAuctionCommandArgs): Observable<RequestStatus> {
    const url = '/api/userUpdateAuction';
    const req = this.httpClient.post(url, { ...args });
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.WSQueuedCommand);
  }

}
