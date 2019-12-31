import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WSCommandStatusService, RequestStatus } from '../../services/WSCommandStatusService';
import { Observable, of } from 'rxjs';
import { CommandHelper, ResponseOptions } from '../ComandHelper';


@Injectable({
  providedIn: 'root'
})
export class RemoveAuctionImageCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {

  }

  execute(imgNum: number): Observable<RequestStatus> {
    const url = `/api/removeAuctionImage?num=${imgNum}`;
    const req = this.httpClient.post(url, null);
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.HTTPQueuedCommand);
  }
}
