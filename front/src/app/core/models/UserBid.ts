export interface UserBid {
  auctionId: string;
  auctionName: string;
  price: number;
  dateCreated: Date;
  auctionCanceled: boolean;
  auctionCompleted: boolean;
  bidId: string;
  bidCanceled: boolean;
}
