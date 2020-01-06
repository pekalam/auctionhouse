import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WSCommandStatusService, RequestStatus } from '../../services/WSCommandStatusService';
import { Observable, of } from 'rxjs';
import { CommandHelper, ResponseOptions } from '../ComandHelper';
import { environment } from '../../../../environments/environment';


@Injectable({
  providedIn: 'root'
})
export class RemoveAuctionImageCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {

  }

  execute(imgNum: number): Observable<RequestStatus> {
    const url = `${environment.API_URL}/api/removeAuctionImage?num=${imgNum}`;
    const req = this.httpClient.post(url, null);
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.HTTPQueuedCommand);
  }
}
