import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Category } from '../models/Category';
import { Observable } from 'rxjs';
import { AuctionListModel } from '../models/Auctions';
import { QueryHelper } from './QueryHelper';
import { environment } from '../../../environments/environment';

export enum ConditionQuery {
  Used, New, All
}

export enum AuctionTypeQuery {
  BuyNowOnly, Auction, AuctionAndBuyNow, All
}

export class AuctionFilters {
  constructor(public condition: ConditionQuery = ConditionQuery.All,
    public auctionType: AuctionTypeQuery = AuctionTypeQuery.All, public minBuyNow = "0", public maxBuyNow = "0",
    public minAuction = "0", public maxAuction = "0") { }

  equals(filter: AuctionFilters): boolean {
    return filter.auctionType == this.auctionType && filter.condition == this.condition;
  }
}

export interface AuctionsQueryResult{
  auctions: AuctionListModel[];
  total: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuctionsQuery {
  constructor(private httpClient: HttpClient, private queryHelper: QueryHelper) {
  }

  private getCondition(c: ConditionQuery): string {
    return `cond=${c}`;
  }

  private getCategoryList(category: Category): string {
    let next: Category = category;
    let list = '';
    while (next) {
      list += `categories=${encodeURIComponent(next.name)}`;
      next = next.subCategory;
      if (next) {
        list += '&';
      }
    }
    return list;
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

  execute(page: number, category: Category, filters?: AuctionFilters): Observable<AuctionsQueryResult> {
    if (!filters) { filters = new AuctionFilters(); }
    const url = `${environment.API_URL}/api/q/auctions?page=${page}&${this.getCategoryList(category)}&${this.getCondition(filters.condition)}`
    + `&${this.getAuctionTypeQuery(filters)}&${this.getAuctionBuyNowPrice(filters)}&${this.getAuctionPrice(filters)}`;
    return this.queryHelper.pipeLoading(this.httpClient.get<AuctionsQueryResult>(url, {}));
  }
}
