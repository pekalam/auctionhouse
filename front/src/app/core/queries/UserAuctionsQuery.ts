import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Auction } from '../models/Auctions';
import { QueryHelper } from './QueryHelper';

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

  execute(pageNum: number): Observable<UserAuctions> {
    const url = `/api/userAuctions?page=${pageNum}`;
    let req = this.httpClient.get<UserAuctions>(url);
    return this.queryHelper.pipeLoading(req);
  }
}
