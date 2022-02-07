import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { TopAuctionQueryItem } from './TopAuctionsByTagQuery';
import { environment } from '../../../environments/environment';


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

  execute(productName: string, page: number): Observable<TopAuctionsByProductNameQueryResult[]>{
    const url = `${environment.API_URL}/api/q/topAuctionsByProductName?product-name=${productName}&page=${page}`;
    return this.httpClient.get<TopAuctionsByProductNameQueryResult[]>(url, {});
  }
}
