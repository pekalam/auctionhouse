import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Auction } from '../models/Auctions';
import { QueryHelper } from './QueryHelper';
import { environment } from '../../../environments/environment';

export enum UserAuctionsSorting{
  DATE_CREATED = 0
}

export enum UserAuctionsSortDir{
  DESCENDING = 0, ASCENDING
}

export interface UserAuctions {
  auctions: Auction[];
  total: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserAuctionsQuery {
  constructor(private httpClient: HttpClient, private queryHelper: QueryHelper) {
  }

  execute(pageNum: number, showArchived: boolean, sorting: UserAuctionsSorting, dir: UserAuctionsSortDir): Observable<UserAuctions> {
    const url = `${environment.API_URL}/api/q/userAuctions?page=${pageNum}&show-archived=${showArchived}&sort=${sorting}&dir=${dir}`;
    let req = this.httpClient.get<UserAuctions>(url);
    return this.queryHelper.pipeLoading(req);
  }
}
