import { Category } from './Category';
import { UserIdentity } from './UserIdentity';
import { Bid } from './Bid';
import { Product, Condition } from './Product';

export interface AuctionImage{
  size1Id: string;
  size2Id: string;
  size3Id: string;
}

export interface AuctionListModel{
  id: string;
  auctionId: string;
  creator: UserIdentity;
  productName: string;
  category: Category;
  startDate: Date;
  endDate: Date;
  buyNowPrice: number;
  actualPrice: number;
  totalBids: number;
  buyNowOnly: boolean;
  condition: Condition;
  auctionImages: AuctionImage[];
}

export interface Auction {
  id: string;
  auctionId: string;
  creator: UserIdentity;
  product: Product;
  category: Category;
  startDate: Date;
  endDate: Date;
  buyNowOnly: boolean;
  buyNowPrice: number;
  actualPrice: number;
  winningBid: Bid;
  completed: boolean;
  buyer: UserIdentity;
  bought: boolean;
  auctionImages: AuctionImage[];
  totalBids: number;
  views: number;
}
