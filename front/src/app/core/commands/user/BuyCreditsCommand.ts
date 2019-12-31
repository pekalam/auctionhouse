import { Injectable } from '@angular/core';
import { RequestStatus, WSCommandStatusService } from '../../services/WSCommandStatusService';
import { Observable, of } from 'rxjs';
import { HttpHeaders, HttpClient } from '@angular/common/http';
import { filter, switchMap, map, catchError } from 'rxjs/operators';
import { CommandHelper, ResponseOptions } from '../ComandHelper';


@Injectable({
  providedIn: 'root'
})
export class BuyCreditsCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(): Observable<RequestStatus> {
    const url = '/api/buyCredits';
    const req = this.httpClient.post(url, {});
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.WSQueuedCommand);
  }
}
