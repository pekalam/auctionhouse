import { Subject, Observable, of } from 'rxjs';
import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { RequestStatus } from './WSCommandStatusService';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { setTimeout } from 'timers';

export const MAX_RETRY = 3;
export const INTERVAL_SEC = 1000;

@Injectable({
  providedIn: 'root'
})
export class HTTPCommandStatusService {

  private handlerMap = new Map<string, number>();

  constructor(private httpClient: HttpClient) { }

  private clearHandlerInterval(commandId: string) {
    const interval = this.handlerMap.get(commandId);
    console.log('clear ' + interval);

    window.clearInterval(interval);
    this.handlerMap.delete(commandId);
  }

  private sendCommandStatusRequest(commandId: string): Observable<RequestStatus> {
    const url = `${environment.API_URL}/api/s/status/${commandId}`;
    return this.httpClient.get<RequestStatus>(url);
  }


  private setRetryTimeout(func: () => void, commandId: string, intervalSec: number, retry: number) {
    const timeout = window.setTimeout(func, intervalSec * retry);
    this.handlerMap.set(commandId, timeout);
  }

  setupServerMessageHandler(commandId: string,
    intervalSec: number = INTERVAL_SEC,
    maxRetry = MAX_RETRY): Observable<RequestStatus> {
    const statusSubj = new Subject<RequestStatus>();
    let retry = 0;

    const timeoutFunc = () => {
      this.sendCommandStatusRequest(commandId).subscribe((status) => {
        console.log(status);
        if (status.status === 'PENDING') {
          retry++;
          if (retry === maxRetry) {
            this.clearHandlerInterval(commandId);
            statusSubj.error(null);
          } else {
            this.setRetryTimeout(timeoutFunc, commandId, intervalSec, retry);
          }
        } else {
          this.clearHandlerInterval(commandId);
          statusSubj.next(status);
        }
      }, (err: HttpErrorResponse) => {
        console.log(err);
        if (err.status === 404) {
          retry++;
          if (retry === maxRetry) {
            this.clearHandlerInterval(commandId);
            statusSubj.error(null);
          } else {
            this.setRetryTimeout(timeoutFunc, commandId, intervalSec, retry);
          }
        } else {
          this.clearHandlerInterval(commandId);
          statusSubj.error(err);
        }
      });
    };
    setTimeout(() => timeoutFunc(), 0);
    return statusSubj.asObservable();
  }
}
