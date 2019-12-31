import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Auction } from '../models/Auctions';

export interface UserAuctions {
  auctions: Auction[];
}

@Injectable({
  providedIn: 'root'
})
export class UserAuctionsQuery {
  constructor(private httpClient: HttpClient) {
  }

  execute(pageNum: number): Observable<UserAuctions> {
    const url = `/api/userAuctions?page=${pageNum}`;
    return this.httpClient.get<UserAuctions>(url);
  }
}
