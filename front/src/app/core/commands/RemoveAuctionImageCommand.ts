import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ServerMessageService } from '../services/ServerMessageService';
import { Observable } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export class RemoveAuctionImageCommand{
  constructor(private httpClient: HttpClient, private serverMessageService: ServerMessageService){

  }

  execute(imgNum: number) : Observable<any>{
    const url = `/api/removeAuctionImage?num=${imgNum}`;
    return this.httpClient.post(url, null);
  }
}
