import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { AuctionCreateStep } from '../auctionCreateStep';
import { AuctionDataStep } from './auctionDataStep';
import { FormGroup, Validators, FormControl } from '@angular/forms';
import { MatDatepicker } from '@angular/material';
import {MomentDateAdapter} from '@angular/material-moment-adapter';


export interface AuctionDataFormValues {
  name: string;
  startDate: Date;
  endDate: Date;
  buyNow: boolean;
  auction: boolean;
  buyNowPrice: number;
}


@Component({
  selector: 'app-auction-data-step',
  templateUrl: './auction-data-step.component.html',
  styleUrls: ['./auction-data-step.component.scss']
})
export class AuctionDataStepComponent extends AuctionCreateStep<AuctionDataStep> implements OnInit {

  @ViewChild('startDatepicker', {static: true}) startDatepicker: MatDatepicker<MomentDateAdapter>;
  @ViewChild('endDatepicker', {static: true}) endDatepicker: MatDatepicker<MomentDateAdapter>;


  @Input('disable')
  set setDisable(obj) {
    Object.assign(this.disable, obj);
  }

  disable: { startDate: boolean } = { startDate: false };

  @Input('defaults')
  set setDefaults(defaults: AuctionDataFormValues) {
    if (defaults) {
      console.log(defaults);

      this.form.setValue({ ...defaults });
      this.onAuctionChange(defaults.auction);
      this.onBuyNowChange(defaults.buyNow);
      this.defaultEndDate = defaults.endDate;
      this.defaultStartDate = defaults.startDate;
      this.ready = this.form.valid;
    }
  }

  titleMsg = 'Auction data';
  defaultStartDate: Date;
  defaultEndDate: Date;
  form = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(5)]),
    startDate: new FormControl(this.defaultStartDate, [Validators.required]),
    endDate: new FormControl(this.defaultEndDate, [Validators.required]),
    buyNow: new FormControl(false, []),
    auction: new FormControl(true, []),
    buyNowPrice: new FormControl({ disabled: true, value: '' }, [Validators.required]),
  });

  constructor() {
    super();
    this.defaultStartDate = new Date();
    const nextMonth = new Date();
    nextMonth.setMonth(nextMonth.getMonth() + 1);
    this.defaultEndDate = nextMonth;
  }

  onAuctionChange(checked) {
    if (!checked && !this.form.controls.buyNow.value) {
      this.form.controls.auction.setValue(true);
      this.form.controls.auction.updateValueAndValidity();

    }
  }

  onBuyNowChange(checked) {
    if (checked) {
      this.form.controls.buyNowPrice.enable();
      this.form.controls.buyNowPrice.setValidators([Validators.required]);
      this.form.controls.buyNowPrice.updateValueAndValidity();
    } else {
      if (!this.form.controls.auction.value) {
        this.form.controls.buyNowPrice.setValue(true);
        this.form.controls.buyNowPrice.updateValueAndValidity();
        return;
      }
      this.form.controls.buyNowPrice.disable();
      this.form.controls.buyNowPrice.reset();
    }
  }

  onChange() {
    console.log(this.form);

    this.ready = this.form.valid;
  }

  ngOnInit() {
  }

  onOkClick() {
    if (this.form.valid) {
/*       console.log(this.form.value.endDate);
      console.log(this.form.value.endDate.toObject());
      console.log(this.form.value.endDate.toString());
      console.log(JSON.stringify(this.form.value.endDate));
      console.log(typeof this.form.value.endDate); */



      let step: AuctionDataStep = {
        name: this.form.value.name,
        buyNow: this.form.value.buyNow,
        buyNowPrice: this.form.value.buyNowPrice,
        endDate: this.form.value.endDate,
        startDate: this.form.value.startDate,
        auction: this.form.value.auction
      };
      console.log(step);

      this.completeStep(step)
    }

  }

}
