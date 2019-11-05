import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';


export interface TopAuctionQueryItem {
  auctionId: string;
  auctionName: string;
}

export interface TopAuctionsQueryResult {
  tag: string;
  total: number;
  auctions: TopAuctionQueryItem[];
}

@Injectable({
  providedIn: 'root'
})
export class TopAuctionsByTagQuery {
  constructor(private httpClient: HttpClient) {
  }

  execute(tag: string, page: number): Observable<TopAuctionsQueryResult>{
    const url = `/api/topAuctionsByTag?tag=${tag}&page=${page}`;
    return this.httpClient.get<TopAuctionsQueryResult>(url, {});
  }
}