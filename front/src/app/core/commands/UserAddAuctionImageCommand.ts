import { Injectable } from '@angular/core';
import { ServerMessage, ServerMessageService } from '../services/ServerMessageService';
import { Observable, of } from 'rxjs';
import { HttpHeaders, HttpClient } from '@angular/common/http';
import { filter, catchError, switchMap } from 'rxjs/operators';


@Injectable({
  providedIn: 'root'
})
export class UserAddAuctionImageCommand {
  constructor(private httpClient: HttpClient, private serverMessageService: ServerMessageService) {
  }

  execute(event: FileList, imgNum: number, auctionId: string): Observable<ServerMessage> {
    const url = `/api/userAddAuctionImage`;
    const file: File = event && event.item(0);
    const formData = new FormData();
    formData.append('img', file);
    formData.append('auction-id', auctionId);
    console.log(formData);

    const httpHeaders = new HttpHeaders({ 'enctype': 'multipart/form-data' });

    return this.httpClient.post(url, formData, { headers: httpHeaders })
      .pipe(
        catchError((err) => {
          console.log(err);
          return of(err);
        }),
        switchMap((response: ServerMessage) => {
          console.log(response);
          if (response.status === "COMPLETED") {
            return of(response);
          }
          let handler = this.serverMessageService.setupServerMessageHandler(response.correlationId);
          return handler.pipe(filter((v) => v.correlationId === response.correlationId));
        })
      );
  }
}
