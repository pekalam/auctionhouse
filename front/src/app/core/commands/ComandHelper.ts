import { Observable, of } from 'rxjs';
import { RequestStatus } from '../services/WSCommandStatusService';
import { Injectable } from '@angular/core';
import { HTTPCommandHelper } from './HTTPCommandHelper';
import { WSCommandHelper } from './WSCommandHelper';

export enum ResponseOptions {
  WSQueuedCommand, HTTPQueuedCommand
}

@Injectable({
  providedIn: 'root'
})
export class CommandHelper {

  constructor(private httpCommandHelper: HTTPCommandHelper, private wsCommandHelper: WSCommandHelper) {
  }

  getResponseStatusHandler(commandRequest: any, showLoading: boolean = false,
    responseOpt: ResponseOptions,
    opt?: { intervalSec: number, maxRetry: number }): Observable<RequestStatus> {
    if (responseOpt === ResponseOptions.HTTPQueuedCommand) {
      if (opt) {
        return this.httpCommandHelper.getResponseStatusHandler(commandRequest, showLoading, opt.intervalSec, opt.maxRetry);
      } else {
        return this.httpCommandHelper.getResponseStatusHandler(commandRequest, showLoading);
      }
    } else if (responseOpt === ResponseOptions.WSQueuedCommand) {
      return this.wsCommandHelper.getResponseStatusHandler(commandRequest, showLoading);
    }
  }
}
