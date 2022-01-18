import { Observable, of } from 'rxjs';
import { RequestStatus, WSCommandStatusService } from '../services/WSCommandStatusService';
import { catchError, switchMap, filter, tap } from 'rxjs/operators';
import { LoadingService } from '../services/LoadingService';
import { Injectable } from '@angular/core';


@Injectable({
  providedIn: 'root'
})
export class WSCommandHelper {
  constructor(private serverMessageService: WSCommandStatusService, private loadingService: LoadingService) {}

  getResponseStatusHandler(commandRequest: any, showLoading: boolean = false): Observable<RequestStatus> {
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
        const handler = this.serverMessageService.setupServerMessageHandler(response.commandId);
        return handler.pipe(filter((v) => v.commandId === response.commandId), tap(() => this.loadingService.setLoading(false)));
      })
    );
  }
}
