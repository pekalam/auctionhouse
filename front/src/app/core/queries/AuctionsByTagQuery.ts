import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuctionListModel } from '../models/Auctions';
import { AuctionFilters, ConditionQuery } from './AuctionsQuery';



@Injectable({
  providedIn: 'root'
})
export class AuctionsByTagQuery {
  constructor(private httpClient: HttpClient) {
  }

  private getCondition(c: ConditionQuery): string {
    return `cond=${c}`;
  }

  private getAuctionTypeQuery(filter: AuctionFilters): string {
    return `type=${filter.auctionType}`;
  }

  private getAuctionBuyNowPrice(filter: AuctionFilters): string {
    return `minbpr=${filter.minBuyNow}&maxbpr=${filter.maxBuyNow}`;
  }

  private getAuctionPrice(filter: AuctionFilters): string {
    return `minapr=${filter.minAuction}&maxapr=${filter.maxAuction}`;
  }

  execute(page: number, tag: string, filters?: AuctionFilters): Observable<AuctionListModel[]> {
    if (!filters) { filters = new AuctionFilters(); }
    const url = `/api/auctionsByTag?page=${page}&tag=${tag}&${this.getCondition(filters.condition)}`
    + `&${this.getAuctionTypeQuery(filters)}&${this.getAuctionBuyNowPrice(filters)}&${this.getAuctionPrice(filters)}`;
    return this.httpClient.get<AuctionListModel[]>(url, {});
  }
}
