import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { AuctionCreateStep } from '../auctionCreateStep';
import { AuctionDataStep } from './auctionDataStep';
import { FormGroup, Validators, FormControl } from '@angular/forms';
import { MatDatepicker } from '@angular/material';
import { MomentDateAdapter } from '@angular/material-moment-adapter';


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

  @ViewChild('startDatepicker', { static: true }) startDatepicker: MatDatepicker<MomentDateAdapter>;
  @ViewChild('endDatepicker', { static: true }) endDatepicker: MatDatepicker<MomentDateAdapter>;


  @Input('disable')
  set setDisable(obj) {
    Object.assign(this.disable, obj);
  }

  disable: { startDate: boolean } = { startDate: false };

  @Input('defaults')
  set setDefaults(defaults: AuctionDataFormValues) {
    if (defaults) {
      console.log(defaults);
      defaults.startDate = new Date(defaults.startDate);
      defaults.endDate = new Date(defaults.endDate);


      this.defaultEndDate = defaults.endDate;
      this.defaultStartDate = defaults.startDate;
      this.form.setValue({ ...defaults, startTime: this.getTimeStr(this.defaultStartDate), endTime: this.getTimeStr(this.defaultEndDate) });
      this.onAuctionChange(defaults.auction);
      this.onBuyNowChange(defaults.buyNow);
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
    startTime: new FormControl('', []),
    endTime: new FormControl('', []),
  });

  constructor() {
    super();
    this.defaultStartDate = new Date();
    const nextMonth = new Date();
    nextMonth.setMonth(nextMonth.getMonth() + 1);
    this.defaultEndDate = nextMonth;
    this.setDefaultTime();
  }

  private setDefaultTime() {
    this.form.controls.startTime.setValue(this.getTimeStr(this.defaultStartDate));
    this.form.controls.endTime.setValue(this.getTimeStr(this.defaultEndDate));
  }

  private getTimeStr(date: Date): string {
    return (date.getHours() < 10 ? '0' + date.getHours() : date.getHours())
      + ':' + (date.getMinutes() < 10 ? '0' + date.getMinutes() : date.getMinutes());
  }

  onAuctionChange(checked) {
    if (!checked && !this.form.controls.buyNow.value) {
      this.form.controls.auction.setValue(true);
      this.form.controls.auction.updateValueAndValidity();
    }
    this.onChange();
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
    this.onChange();
  }

  onChange() {
    console.log(this.form);

    this.ready = this.form.valid;
  }

  ngOnInit() {
  }

  onOkClick() {
    if (this.form.valid) {
      const startDate: Date = this.form.value.startDate.toDate ? this.form.value.startDate.toDate() : this.form.value.startDate;
      startDate.setHours(this.form.value.startTime.split(':')[0]);
      startDate.setMinutes(this.form.value.startTime.split(':')[1]);
      const endDate: Date = this.form.value.endDate.toDate ? this.form.value.endDate.toDate() : this.form.value.endDate;
      endDate.setHours(this.form.value.endTime.split(':')[0]);
      endDate.setMinutes(this.form.value.endTime.split(':')[1]);
      const step: AuctionDataStep = {
        name: this.form.value.name,
        buyNow: this.form.value.buyNow,
        buyNowPrice: this.form.value.buyNowPrice,
        endDate,
        startDate,
        auction: this.form.value.auction
      };
      console.log(step);

      this.completeStep(step);
    }

  }

}
