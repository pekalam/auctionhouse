import { Category } from './Category';
import { UserIdentity } from './UserIdentity';
import { Bid } from './Bid';
import { Product, Condition } from './Product';

export interface AuctionImage{
  imgNum: number;
  size1Id: string;
  size2Id: string;
  size3Id: string;
}

export interface AuctionListModel{
  id: string;
  auctionId: string;
  owner: UserIdentity;
  productName: string;
  name: string;
  category: Category;
  startDate: Date;
  endDate: Date;
  buyNowPrice: string;
  actualPrice: string;
  totalBids: number;
  buyNowOnly: boolean;
  condition: Condition;
  auctionImages: AuctionImage[];
}

export interface Auction {
  id: string;
  auctionId: string;
  owner: UserIdentity;
  product: Product;
  category: Category;
  startDate: Date;
  endDate: Date;
  buyNowOnly: boolean;
  buyNowPrice: string;
  actualPrice: string;
  winningBid?: Bid;
  completed: boolean;
  buyer: UserIdentity;
  bought: boolean;
  auctionImages: AuctionImage[];
  totalBids: number;
  views: number;
  tags: string[];
  name: string;
  dateCreated: Date;
  archived: boolean;
}
