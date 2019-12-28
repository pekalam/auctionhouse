import { Injectable } from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserBid } from '../models/UserBid';

export interface UserBidsQueryResult{
  userBids: UserBid[];
}

@Injectable({
  providedIn: 'root'
})
export class UserBidsQuery{
  constructor(private httpClient: HttpClient) {
  }

  execute(page: number = 0): Observable<UserBidsQueryResult>{
    const url = `/api/userBids?page=${page}`;
    return this.httpClient.get<UserBidsQueryResult>(url);
  }
}
