import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Auction } from '../models/Auctions';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { QueryHelper } from './QueryHelper';

@Injectable({
  providedIn: 'root'
})
export class AuctionQuery {
  constructor(private httpClient: HttpClient, private queryHelper: QueryHelper) {

  }

  execute(auctionId: string): Observable<Auction>{
    const url = `${environment.API_URL}/api/q/auction?auctionId=${auctionId}`;
    return this.queryHelper.pipeLoading(this.httpClient.get<Auction>(url, {}));
  }
}
