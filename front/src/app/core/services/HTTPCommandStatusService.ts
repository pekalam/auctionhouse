import { Subject, Observable, of } from 'rxjs';
import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { RequestStatus } from './WSCommandStatusService';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export const MAX_RETRY = 3;
export const INTERVAL_SEC = 1000;

@Injectable({
  providedIn: 'root'
})
export class HTTPCommandStatusService {

  private handlerMap = new Map<string, number>();

  constructor(private httpClient: HttpClient) { }

  private clearHandlerInterval(correlationId: string) {
    const interval = this.handlerMap.get(correlationId);
    console.log('clear ' + interval);

    window.clearInterval(interval);
    this.handlerMap.delete(correlationId);
  }

  private sendCommandStatusRequest(correlationId: string): Observable<RequestStatus> {
    const url = `${environment.API_URL}/api/command/${correlationId}`;
    return this.httpClient.get<RequestStatus>(url);
  }


  private setRetryTimeout(func: () => void, correlationId: string, intervalSec: number, retry: number) {
    const timeout = window.setTimeout(func, intervalSec * retry);
    this.handlerMap.set(correlationId, timeout);
  }

  setupServerMessageHandler(correlationId: string,
    intervalSec: number = INTERVAL_SEC,
    maxRetry = MAX_RETRY): Observable<RequestStatus> {
    const statusSubj = new Subject<RequestStatus>();
    let retry = 0;

    const timeoutFunc = () => {
      this.sendCommandStatusRequest(correlationId).subscribe((status) => {
        console.log(status);
        if (status.status === 'PENDING') {
          retry++;
          if (retry === maxRetry) {
            this.clearHandlerInterval(correlationId);
            statusSubj.error(null);
          } else {
            this.setRetryTimeout(timeoutFunc, correlationId, intervalSec, retry);
          }
        } else {
          this.clearHandlerInterval(correlationId);
          statusSubj.next(status);
        }
      }, (err: HttpErrorResponse) => {
        console.log(err);
        if (err.status === 404) {
          retry++;
          if (retry === maxRetry) {
            this.clearHandlerInterval(correlationId);
            statusSubj.error(null);
          } else {
            this.setRetryTimeout(timeoutFunc, correlationId, intervalSec, retry);
          }
        } else {
          this.clearHandlerInterval(correlationId);
          statusSubj.error(err);
        }
      });
    };
    timeoutFunc();
    return statusSubj.asObservable();
  }
}
