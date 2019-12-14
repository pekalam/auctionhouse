import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ServerMessageService, ServerMessage } from '../services/ServerMessageService';
import { Observable, of } from 'rxjs';
import { catchError, switchMap, filter } from 'rxjs/operators';
import { CommandHelper } from './CommandHelper';


@Injectable({
  providedIn: 'root'
})
export class RemoveAuctionImageCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {

  }

  execute(imgNum: number): Observable<ServerMessage> {
    const url = `/api/removeAuctionImage?num=${imgNum}`;
    return this.commandHelper.getResponseStatusHandler(this.httpClient.post(url, null), true);
  }
}
