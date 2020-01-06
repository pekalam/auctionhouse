import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuctionListModel } from '../models/Auctions';
import { AuctionFilters, ConditionQuery, AuctionsQueryResult } from './AuctionsQuery';
import { QueryHelper } from './QueryHelper';
import { environment } from '../../../environments/environment';



@Injectable({
  providedIn: 'root'
})
export class AuctionsByTagQuery {
  constructor(private httpClient: HttpClient, private queryHelper: QueryHelper) {
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

  execute(page: number, tag: string, filters?: AuctionFilters): Observable<AuctionsQueryResult> {
    if (!filters) { filters = new AuctionFilters(); }
    const url = `${environment.API_URL}/api/auctionsByTag?page=${page}&tag=${tag}&${this.getCondition(filters.condition)}`
    + `&${this.getAuctionTypeQuery(filters)}&${this.getAuctionBuyNowPrice(filters)}&${this.getAuctionPrice(filters)}`;
    return this.queryHelper.pipeLoading(this.httpClient.get<AuctionsQueryResult>(url, {}));
  }
}
