import { Injectable } from '@angular/core';
import { RequestStatus, WSCommandStatusService } from '../../services/WSCommandStatusService';
import { Observable, of } from 'rxjs';
import { HttpHeaders, HttpClient } from '@angular/common/http';
import { filter, switchMap, map, catchError } from 'rxjs/operators';
import { CommandHelper, ResponseOptions } from '../ComandHelper';
import { environment } from '../../../../environments/environment';


@Injectable({
  providedIn: 'root'
})
export class BuyCreditsCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(ammount: number): Observable<RequestStatus> {
    const url = `${environment.API_URL}/api/c/buyCredits`;
    const req = this.httpClient.post(url, {ammount});
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.HTTPQueuedCommand);
  }
}
