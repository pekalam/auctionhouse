import { Injectable } from '@angular/core';
import { ServerMessage, ServerMessageService } from '../services/ServerMessageService';
import { Observable, of } from 'rxjs';
import { HttpHeaders, HttpClient } from '@angular/common/http';
import { filter, switchMap, map, catchError } from 'rxjs/operators';
import { CommandHelper } from './CommandHelper';


@Injectable({
  providedIn: 'root'
})
export class AddAuctionImageCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(event: FileList, imgNum: number): Observable<ServerMessage> {
    const url = '/api/addAuctionImage';
    const file: File = event && event.item(0);
    const formData = new FormData();
    formData.append('img', file);
    formData.append('img-num', imgNum.toString());
    const httpHeaders = new HttpHeaders({ 'enctype': 'multipart/form-data' });


    return this.commandHelper.getResponseStatusHandler(this.httpClient.post(url, formData, { headers: httpHeaders }), true);
  }
}
