import { Component, OnInit } from '@angular/core';
import { FormGroup, Validators, FormControl } from '@angular/forms';
import { Product, Condition } from 'src/app/core/models/Product';
import { Auction } from '../../../../../core/models/Auctions';
import { AuctionCreateStep } from '../../../../auctionCreateStep';
import { ProductStep } from 'src/app/auctions/productStep';

@Component({
  selector: 'app-product-step',
  templateUrl: './product-step.component.html',
  styleUrls: ['./product-step.component.scss']
})
export class ProductStepComponent extends AuctionCreateStep<ProductStep> implements OnInit {

  form = new FormGroup({
    startDate: new FormControl('', [Validators.required]),
    endDate: new FormControl('', [Validators.required]),
    productName: new FormControl('', [Validators.required]),
    productDescription: new FormControl('', [Validators.required]),
    buyNowPrice: new FormControl('', []),
    buyNow: new FormControl(false, []),
  });

  constructor() { super(); }

  ngOnInit() {
  }

  onSubmit() {
    console.log(this.form.value);
    console.log(this.form.valid);
    console.log(this.form.errors);


    if (this.form.valid) {
      let product: Product = {
        name: this.form.value.productName,
        description: this.form.value.productDescription,
        condition: Condition.New
      };
      let startDate = this.form.value.startDate;
      let endDate = this.form.value.endDate;
      let buyNow = this.form.value.buyNow;
      let buyNowPrice = this.form.value.buyNowPrice;
      this.completeStep(new ProductStep(product, startDate, endDate, buyNowPrice, buyNow));
    }
  }

}


