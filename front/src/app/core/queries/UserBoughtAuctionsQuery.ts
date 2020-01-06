import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Auction } from '../models/Auctions';
import { Observable } from 'rxjs';
import { QueryHelper } from './QueryHelper';
import { environment } from '../../../environments/environment';
import { UserAuctionsSorting, UserAuctionsSortDir } from './UserAuctionsQuery';

export interface UserBoughtAuctions {
  auctions: Auction[];
  total: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserBoughtAuctionsQuery {
  constructor(private httpClient: HttpClient, private queryHelper: QueryHelper) {

  }

  execute(page: number, sorting: UserAuctionsSorting, dir: UserAuctionsSortDir): Observable<UserBoughtAuctions> {
    const url = `${environment.API_URL}/api/userBoughtAuctions?page=${page}&sort=${sorting}&dir=${dir}`;
    let req = this.httpClient.get<UserBoughtAuctions>(url);
    return this.queryHelper.pipeLoading(req);
  }
}
