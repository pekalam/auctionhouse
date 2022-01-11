import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';


export interface TopAuctionQueryItem {
  auctionId: string;
  auctionName: string;
  auctionImage: {size1Id: string, size2Id: string, size3Id: string};
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
    const url = `${environment.API_URL}/api/q/topAuctionsByTag?tag=${tag}&page=${page}`;
    return this.httpClient.get<TopAuctionsQueryResult>(url, {});
  }
}
