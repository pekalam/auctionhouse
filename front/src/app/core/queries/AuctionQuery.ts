import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Auction } from '../models/Auctions';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuctionQuery {
  constructor(private httpClient: HttpClient) {

  }

  execute(auctionId: string): Observable<Auction>{
    const url = `/api/auction?auctionId=${auctionId}`;
    return this.httpClient.get<Auction>(url, {});
  }
}
