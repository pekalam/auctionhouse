import { Observable, of } from 'rxjs';
import { RequestStatus, WSCommandStatusService } from '../../services/WSCommandStatusService';
import { filter, catchError, switchMap } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { WSCommandHelper } from '../WSCommandHelper';
import { ResponseOptions, CommandHelper } from '../ComandHelper';
import { environment } from '../../../../environments/environment';


@Injectable({
  providedIn: 'root'
})
export class StartAuctionCreateSessionCommand {

  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute() {
    const url = `${environment.API_URL}/api/c/startCreateSession`;
    const req = this.httpClient.post(url, {});
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.WSQueuedCommand);
  }
}
