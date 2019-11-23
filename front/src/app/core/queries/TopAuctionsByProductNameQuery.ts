import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { TopAuctionQueryItem } from './TopAuctionsByTagQuery';


export interface TopAuctionsByProductNameQueryResult {
  canonicalName: string;
  total: number;
  auctions: TopAuctionQueryItem[];
}

@Injectable({
  providedIn: 'root'
})
export class TopAuctionsByProductNameQuery {
  constructor(private httpClient: HttpClient) {
  }

  execute(productName: string, page: number): Observable<TopAuctionsByProductNameQueryResult>{
    const url = `/api/topAuctionsByProductName?product-name=${productName}&page=${page}`;
    return this.httpClient.get<TopAuctionsByProductNameQueryResult>(url, {});
  }
}
