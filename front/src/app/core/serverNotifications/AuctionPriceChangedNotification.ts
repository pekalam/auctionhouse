import { Bid } from '../models/Bid';

export function AuctionPriceChangedNotificationName(auctionId: string){
  return `AuctionPriceChanged-${auctionId}`;
}

export interface AuctionPriceChangedNotification {
  newPrice: string;
  auctionId: string;
  bidId: string;
  winnerId: string;
  dateCreated: Date;
}
