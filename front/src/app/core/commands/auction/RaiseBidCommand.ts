import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RequestStatus } from '../../services/WSCommandStatusService';
import { Observable } from 'rxjs';
import { CommandHelper, ResponseOptions } from '../ComandHelper';
import { environment } from '../../../../environments/environment';


@Injectable({
  providedIn: 'root'
})
export class RaiseBidCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(auctionId: string, price: number): Observable<RequestStatus> {
    const url = `${environment.API_URL}/api/c/raiseBid`;
    const req = this.httpClient.post(url, { auctionId, price });
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.HTTPQueuedCommand);
  }
}
