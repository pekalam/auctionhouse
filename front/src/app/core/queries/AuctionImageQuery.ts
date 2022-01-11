import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuctionImage } from '../models/Auctions';
import { environment } from '../../../environments/environment';



@Injectable({
  providedIn: 'root'
})
export class AuctionImageQuery{
  execute(auctionImageId: string): string{
    return `${environment.API_URL}/api/q/auctionImage?img=${auctionImageId}`;
  }
}
