import { Bid } from '../models/Bid';
export const AuctionPriceChangedNotification_Name = 'AuctionPriceChanged';

export interface AuctionPriceChangedNotification {
  winningBid: Bid;
}
