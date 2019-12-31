import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WSCommandStatusService, RequestStatus } from '../../services/WSCommandStatusService';
import { Observable, of } from 'rxjs';
import { CommandHelper, ResponseOptions } from '../ComandHelper';

@Injectable({
  providedIn: 'root'
})
export class BuyNowCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(auctionId: string): Observable<RequestStatus> {
    const url = '/api/buyNow';
    const req = this.httpClient.post(url, { auctionId });
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.HTTPQueuedCommand);
  }
}
