import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CategoryTreeNode } from '../models/CategoryTreeNode';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuctionImage } from '../models/Auctions';


export interface EndingAuctionsQueryResult{
  auctionId: string;
  name: string;
  endDate: Date;
  actualPrice: number;
  totalBids: number;
  views: number;
  minToEnd: number;
  auctionImages: AuctionImage[];
  tags: string[];
}

@Injectable({
  providedIn: 'root'
})
export class EndingAuctionsQuery {
  constructor(private httpClient: HttpClient) {

  }

  execute(): Observable<EndingAuctionsQueryResult[]> {
    const url = `${environment.API_URL}/api/endingAuctions`;
    return this.httpClient.get<EndingAuctionsQueryResult[]>(url, {});
  }
}
