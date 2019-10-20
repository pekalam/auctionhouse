import { UserIdentity } from './UserIdentity';
export interface Bid {
  bidId: string;
  auctionId: string;
  userIdentity: UserIdentity;
  price: number;
  dateCreated: Date;
}
