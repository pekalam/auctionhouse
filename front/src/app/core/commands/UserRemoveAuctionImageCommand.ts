import { Injectable } from '@angular/core';
import { ServerMessage, ServerMessageService } from '../services/ServerMessageService';
import { Observable, of } from 'rxjs';
import { HttpHeaders, HttpClient } from '@angular/common/http';
import { filter, catchError, switchMap } from 'rxjs/operators';
import { CommandHelper } from './CommandHelper';


@Injectable({
  providedIn: 'root'
})
export class UserRemoveAuctionImageCommand {

  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(imgNum: number, auctionId: string): Observable<ServerMessage> {
    const url = `/api/userRemoveAuctionImage`;
    const formData = new FormData();
    formData.append('img-num', imgNum.toString());
    formData.append('auction-id', auctionId);
    console.log(formData);

    const httpHeaders = new HttpHeaders({ 'enctype': 'application/x-www-form-urlencoded' });

    return this.commandHelper.getResponseStatusHandler(this.httpClient.post(url, formData, { headers: httpHeaders }), true);
  }
}
