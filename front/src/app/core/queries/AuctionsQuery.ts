import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Category } from '../models/Category';
import { Observable } from 'rxjs';
import { AuctionListModel } from '../models/Auctions';

export enum Condition {
  "new", "used", "all"
}

@Injectable({
  providedIn: 'root'
})
export class AuctionsQuery {
  constructor(private httpClient: HttpClient) {
  }

  private getCondition(c: Condition): string {
    return `cond=${c}`;
  }

  private getCategoryList(category: Category): string {
    let next: Category = category;
    let list = '';
    while (next) {
      list += `categories=${encodeURIComponent(next.categoryName)}`;
      next = next.subCategory;
      if (next) {
        list += '&';
      }
    }
    return list;
  }

  execute(page: number, category: Category, condition: Condition): Observable<AuctionListModel[]> {
    const url = `/api/auctions?page=${page}&${this.getCategoryList(category)}&${this.getCondition(condition)}`;
    return this.httpClient.get<AuctionListModel[]>(url, {});
  }
}
