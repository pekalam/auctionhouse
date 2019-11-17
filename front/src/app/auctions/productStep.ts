import { Product, Condition } from '../core/models/Product';
export class ProductStep {
  constructor(public product: Product, public tags: string[]){}
}
