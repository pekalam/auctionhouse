import { Observable, of } from 'rxjs';
import { RequestStatus, WSCommandStatusService } from '../services/WSCommandStatusService';
import { catchError, switchMap, filter, tap } from 'rxjs/operators';
import { LoadingService } from '../services/LoadingService';
import { Injectable } from '@angular/core';
import { HTTPCommandStatusService, INTERVAL_SEC, MAX_RETRY } from '../services/HTTPCommandStatusService';


@Injectable({
  providedIn: 'root'
})
export class HTTPCommandHelper {
  constructor(private serverMessageService: HTTPCommandStatusService, private loadingService: LoadingService) { }

  getResponseStatusHandler(commandRequest: any, showLoading: boolean = false,
    intervalSec: number = INTERVAL_SEC,
    maxRetry: number = MAX_RETRY): Observable<RequestStatus> {
    if(showLoading) this.loadingService.setLoading(true);
    return commandRequest.pipe(
      catchError((err, caught) => {
        console.log(err);
        if(showLoading) this.loadingService.setLoading(false);
        throw err;
      }),
      switchMap((response: RequestStatus) => {
        console.log(response);
        if (response.status === 'COMPLETED') {
          if(showLoading) this.loadingService.setLoading(false);
          return of(response);
        }
        const handler = this.serverMessageService.setupServerMessageHandler(response.commandId, intervalSec, maxRetry);
        return handler.pipe(filter((v) => v.commandId === response.commandId), tap(() => this.loadingService.setLoading(false)));
      })
    );
  }
}
