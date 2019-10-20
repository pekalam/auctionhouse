import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuctionImage } from '../models/Auctions';



@Injectable({
  providedIn: 'root'
})
export class AuctionImageQuery{
  execute(auctionImageId: string): string{
    return `/api/auctionImage?img=${auctionImageId}`;
  }
}
