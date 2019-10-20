import { Product } from '../core/models/Product';
export class ProductStep {
  constructor(public product: Product, public startDate: Date, public endDate: Date,
            public buyNowPrice: number, public buyNow: boolean){}
}
