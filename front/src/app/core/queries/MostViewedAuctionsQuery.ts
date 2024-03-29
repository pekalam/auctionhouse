import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Auction, AuctionImage } from '../models/Auctions';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface MostViewedAuction{
  auctionId: string ;
  auctionName: string ;
  startDate: Date ;
  endDate: Date  ;
  buyNowOnly: boolean ;
  buyNowPrice: number ;
  actualPrice: number ;
  totalBids: number ;
  views: number ;
  auctionImages: AuctionImage[];
}

@Injectable({
  providedIn: 'root'
})
export class MostViewedAuctionsQuery {
  constructor(private httpClient: HttpClient) {
  }

  execute(): Observable<MostViewedAuction[]>{
    const url = `${environment.API_URL}/api/q/mostViewedAuctions`;
    return this.httpClient.get<MostViewedAuction[]>(url, {});
  }
}
