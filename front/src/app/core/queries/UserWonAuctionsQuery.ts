import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Auction } from '../models/Auctions';
import { Observable } from 'rxjs';
import { QueryHelper } from './QueryHelper';

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

  execute(page: number): Observable<UserWonAuctions>{
    const url = `/api/userBoughtAuctions?page=${page}`;
    let req = this.httpClient.get<UserWonAuctions>(url);
    return this.queryHelper.pipeLoading(req);
  }
}
