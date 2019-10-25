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

  defaultStartDate: Date;
  defaultEndDate: Date;
  form = new FormGroup({
    startDate: new FormControl(this.defaultStartDate, [Validators.required]),
    endDate: new FormControl(this.defaultEndDate, [Validators.required]),
    productName: new FormControl('', [Validators.required]),
    productDescription: new FormControl('', [Validators.required]),
    buyNowPrice: new FormControl('', []),
    buyNow: new FormControl(false, []),
  });

  constructor() {
    super();
    this.defaultStartDate = new Date();
    let nextMonth = new Date();
    nextMonth.setMonth(nextMonth.getMonth() + 1);
    this.defaultEndDate = nextMonth;
    console.log(this.form);
   }

  ngOnInit() {
  }

  onSubmit() {

  }

  private emitIfReady(){
    if(this.form.valid){
      console.log("valid");
      this.onStepReady.emit();
    }
  }

  onChange(){
    console.log("change");
    this.emitIfReady();
  }

  checkIsReady() {
    this.emitIfReady();
  }

  onOkClick() {
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
      let productStep = new ProductStep(product, startDate, endDate, buyNowPrice, buyNow);
      this.completeStep(productStep);
    }
  }


}


