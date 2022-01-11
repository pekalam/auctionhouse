import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Auction } from '../models/Auctions';
import { Observable } from 'rxjs';
import { QueryHelper } from './QueryHelper';
import { environment } from '../../../environments/environment';
import { UserAuctionsSorting, UserAuctionsSortDir } from './UserAuctionsQuery';

export interface UserWonAuctions{
  auctions: Auction[];
  total: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserWonAuctionsQuery{
  constructor(private httpClient: HttpClient, private queryHelper: QueryHelper) {

  }

  execute(page: number, sorting: UserAuctionsSorting, dir: UserAuctionsSortDir): Observable<UserWonAuctions>{
    const url = `${environment.API_URL}/api/q/userWonAuctions?page=${page}&sort=${sorting}&dir=${dir}`;
    let req = this.httpClient.get<UserWonAuctions>(url);
    return this.queryHelper.pipeLoading(req);
  }
}
