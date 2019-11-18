import { Injectable } from '@angular/core';
import { ServerMessage, ServerMessageService } from '../services/ServerMessageService';
import { Observable } from 'rxjs';
import { HttpHeaders, HttpClient } from '@angular/common/http';
import { filter } from 'rxjs/operators';


@Injectable({
  providedIn: 'root'
})
export class UserAddAuctionImageCommand {
  private handler: Observable<ServerMessage>;

  constructor(private httpClient: HttpClient, private serverMessageService: ServerMessageService) {
    this.handler = this.serverMessageService.setupServerMessageHandler('auctionImageAddedByUser');
  }

  execute(event: FileList, imgNum: number, auctionId: string): Observable<ServerMessage>{
    const url = `/api/userAddAuctionImage`;
    const file: File = event && event.item(0);
    const formData = new FormData();
    formData.append('img', file);
    formData.append('auction-id', auctionId);
    formData.append('correlation-id', '1234');
    console.log(formData);

    const httpHeaders = new HttpHeaders({ 'enctype': 'multipart/form-data' });

    this.httpClient.post(url, formData, { headers: httpHeaders }).subscribe((v) => {
      console.log('post');
    });
    return this.handler.pipe(filter((v) => v.correlationId === '1234'));
  }
}
