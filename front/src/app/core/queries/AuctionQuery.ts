import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Auction } from '../models/Auctions';
import { Observable } from 'rxjs';
import { QueryHelper } from './QueryHelper';

@Injectable({
  providedIn: 'root'
})
export class AuctionQuery {
  constructor(private httpClient: HttpClient, private queryHelper: QueryHelper) {

  }

  execute(auctionId: string): Observable<Auction>{
    const url = `/api/auction?auctionId=${auctionId}`;
    return this.queryHelper.pipeLoading(this.httpClient.get<Auction>(url, {}));
  }
}
