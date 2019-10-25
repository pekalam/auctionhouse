import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Category } from '../models/Category';
import { Observable } from 'rxjs';
import { AuctionListModel } from '../models/Auctions';

export enum ConditionQuery {
  Used, New, All
}

export enum AuctionTypeQuery {
  BuyNow, Auction, All
}

export class AuctionFilters {
  constructor(public condition: ConditionQuery = ConditionQuery.All,
    public auctionType: AuctionTypeQuery = AuctionTypeQuery.All, public minBuyNow = "0", public maxBuyNow = "0",
    public minAuction = "0", public maxAuction = "0") { }

  equals(filter: AuctionFilters): boolean {
    return filter.auctionType == this.auctionType && filter.condition == this.condition;
  }
}



@Injectable({
  providedIn: 'root'
})
export class AuctionsQuery {
  constructor(private httpClient: HttpClient) {
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

  execute(page: number, category: Category, filters?: AuctionFilters): Observable<AuctionListModel[]> {
    if (!filters) { filters = new AuctionFilters(); }
    const url = `/api/auctions?page=${page}&${this.getCategoryList(category)}&${this.getCondition(filters.condition)}`
    + `&${this.getAuctionTypeQuery(filters)}&${this.getAuctionBuyNowPrice(filters)}&${this.getAuctionPrice(filters)}`;
    return this.httpClient.get<AuctionListModel[]>(url, {});
  }
}
