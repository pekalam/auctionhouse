import { Injectable } from '@angular/core';
import { ServerMessage, ServerMessageService } from '../services/ServerMessageService';
import { Observable } from 'rxjs';
import { HttpHeaders, HttpClient } from '@angular/common/http';
import { filter } from 'rxjs/operators';


@Injectable({
  providedIn: 'root'
})
export class AddAuctionImageCommand {
  private handler: Observable<ServerMessage>;

  constructor(private httpClient: HttpClient, private serverMessageService: ServerMessageService) {
    this.handler = serverMessageService.setupServerMessageHandler('auctionImageAdded');
  }

  execute(event: FileList, imgNum: number): Observable<ServerMessage>{
    const url = '/api/addAuctionImage';
    const file: File = event && event.item(0);
    const formData = new FormData();
    formData.append('img', file);
    formData.append('img-num', imgNum.toString());
    formData.append('correlation-id', '1234');
    const httpHeaders = new HttpHeaders({ 'enctype': 'multipart/form-data' });

    this.httpClient.post(url, formData, { headers: httpHeaders }).subscribe((v) => {
      console.log('post');
    });
    return this.handler.pipe(filter((v) => v.correlationId === '1234'));
  }
}
