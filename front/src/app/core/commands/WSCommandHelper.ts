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
    return commandRequest.pipe(
      tap(() => this.loadingService.setLoading(true)),
      catchError((err, caught) => {
        console.log(err);
        this.loadingService.setLoading(false);
        throw err;
      }),
      switchMap((response: RequestStatus) => {
        console.log(response);
        if (response.status === 'COMPLETED') {
          this.loadingService.setLoading(false);
          return of(response);
        }
        const handler = this.serverMessageService.setupServerMessageHandler(response.correlationId);
        return handler.pipe(filter((v) => v.correlationId === response.correlationId), tap(() => this.loadingService.setLoading(false)));
      })
    );
  }
}
