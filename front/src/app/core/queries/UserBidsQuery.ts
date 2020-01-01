import { Injectable } from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserBid } from '../models/UserBid';
import { QueryHelper } from './QueryHelper';

export interface UserBidsQueryResult{
  userBids: UserBid[];
  total: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserBidsQuery{
  constructor(private httpClient: HttpClient, private queryHelper: QueryHelper) {
  }

  execute(page: number = 0): Observable<UserBidsQueryResult>{
    const url = `/api/userBids?page=${page}`;

    let req = this.httpClient.get<UserBidsQueryResult>(url);
    return this.queryHelper.pipeLoading(req);
  }
}
