import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ServerMessageService, ServerMessage } from '../services/ServerMessageService';
import { Observable, of } from 'rxjs';
import { catchError, switchMap, filter } from 'rxjs/operators';


@Injectable({
  providedIn: 'root'
})
export class RemoveAuctionImageCommand {
  constructor(private httpClient: HttpClient, private serverMessageService: ServerMessageService) {

  }

  execute(imgNum: number): Observable<ServerMessage> {
    const url = `/api/removeAuctionImage?num=${imgNum}`;
    return this.httpClient.post(url, null)
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
