import { Component, OnInit } from '@angular/core';
import { FormGroup, Validators, FormControl, ValidatorFn, ValidationErrors, AbstractControl, FormGroupDirective, NgForm } from '@angular/forms';
import { Product, Condition } from 'src/app/core/models/Product';
import { Auction } from '../../../../../core/models/Auctions';
import { AuctionCreateStep } from '../../../../auctionCreateStep';
import { ProductStep } from 'src/app/auctions/productStep';
import { ErrorStateMatcher } from '@angular/material';


function tagsValidator(): ValidatorFn {
  return (control: AbstractControl): { [key: string]: any } | null => {
    const tagsString: string = control.value;
    const tags = tagsString.split(' ').filter((s) => s.length > 0);
    console.log(tags);

    if (tags.length === 0) { return null; }
    if (tags.filter((t) => t.length > 30).length > 0) { return { tagsMaxLength: { value: 'Tag value exceeds max length' } }; }

    return null;
  };

}

export class InstantErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null,
    form: FormGroupDirective | NgForm | null): boolean {
    return control && control.invalid && (control.dirty || control.touched);
  }
}

@Component({
  selector: 'app-product-step',
  templateUrl: './product-step.component.html',
  styleUrls: ['./product-step.component.scss']
})
export class ProductStepComponent extends AuctionCreateStep<ProductStep> implements OnInit {



  titleMsg = 'Basic info';

  defaultStartDate: Date;
  defaultEndDate: Date;
  form = new FormGroup({
    startDate: new FormControl(this.defaultStartDate, [Validators.required]),
    endDate: new FormControl(this.defaultEndDate, [Validators.required]),
    productName: new FormControl('', [Validators.required]),
    productDescription: new FormControl('', [Validators.required]),
    buyNowPrice: new FormControl({ disabled: true, value: '' }, [Validators.required]),
    buyNow: new FormControl(false, []),
    tags: new FormControl('', [Validators.required, tagsValidator()]),
  });
  tags: string[] = [];
  tagsErrorStateMatcher = new InstantErrorStateMatcher();
  showTagsHelp = false;

  constructor() {
    super();
    this.defaultStartDate = new Date();
    const nextMonth = new Date();
    nextMonth.setMonth(nextMonth.getMonth() + 1);
    this.defaultEndDate = nextMonth;
    console.log(this.form);
  }

  ngOnInit() {
  }

  onSubmit() {

  }

  onTagsChange() {
    if (this.form.controls.tags.valid) {
      this.tags = this.form.value.tags.split(' ').filter((s) => s.length > 0);
    } else {
      this.tags = [];
    }
    this.showTagsHelp = this.form.value.tags.split(' ').filter((s) => s.length === 0).length > 1;
  }

  onBuyNowChange(checked) {
    if (checked) {
      this.form.controls.buyNowPrice.enable();
      this.form.controls.buyNowPrice.setValidators([Validators.required]);
      this.form.controls.buyNowPrice.updateValueAndValidity();
    } else {
      this.form.controls.buyNowPrice.disable();
      this.form.controls.buyNowPrice.reset();
    }
  }

  onChange() {
    this.ready = this.form.valid;
  }


  onOkClick() {
    console.log(this.form.value);
    console.log(this.form.valid);
    console.log(this.form.errors);


    if (this.form.valid) {
      const product: Product = {
        name: this.form.value.productName,
        description: this.form.value.productDescription,
        condition: Condition.New
      };
      const startDate = this.form.value.startDate;
      const endDate = this.form.value.endDate;
      const buyNow = this.form.value.buyNow;
      const buyNowPrice = this.form.value.buyNowPrice;
      const productStep = new ProductStep(product, startDate, endDate, buyNowPrice, buyNow, this.tags);
      this.completeStep(productStep);
    }
  }


}


