import { Category } from './Category';
import { UserIdentity } from './UserIdentity';
import { Bid } from './Bid';
import { Product } from './Product';

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
  buyNowPrice: number;
  actualPrice: number;
  winningBid: Bid;
  completed: boolean;
  buyer: UserIdentity;
  bought: boolean;
  auctionImages: AuctionImage[];
}
