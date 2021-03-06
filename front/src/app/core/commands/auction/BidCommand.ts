import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WSCommandStatusService, RequestStatus } from '../../services/WSCommandStatusService';
import { Observable, of } from 'rxjs';
import { CommandHelper, ResponseOptions } from '../ComandHelper';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BidCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(auctionId: string, price: number): Observable<RequestStatus> {
    const url = `${environment.API_URL}/api/bid`;
    const req = this.httpClient.post(url, { auctionId, price });
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.WSQueuedCommand);
  }
}
