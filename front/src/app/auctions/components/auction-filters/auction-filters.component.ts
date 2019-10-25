import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { CategoryTreeNode } from '../../../core/models/CategoryTreeNode';
import { FormGroup, FormBuilder } from '@angular/forms';
import { ConditionQuery, AuctionFilters } from '../../../core/queries/AuctionsQuery';
import {BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';


@Component({
  selector: 'app-auction-filters',
  templateUrl: './auction-filters.component.html',
  styleUrls: ['./auction-filters.component.scss']
})
export class AuctionFiltersComponent implements OnInit {

  private lastFilters: AuctionFilters = null;

  mobile = false;

  @Input()
  subcategories: CategoryTreeNode;

  @Input()
  selectedSubCategory: string;

  @Output()
  applyFilters = new EventEmitter<AuctionFilters>();

  form: FormGroup;
  selectedAuctionType: string = "3";


  constructor(private formBuilder: FormBuilder, public breakpointObserver: BreakpointObserver) {
    this.breakpointObserver
    .observe(['(max-width: 960px)'])
    .subscribe((state: BreakpointState) => {
      if (state.matches) {
        this.mobile = true;
      } else {
        this.mobile = false;
      }
    });
    this.form = formBuilder.group({
      condition: "2",
      type: this.selectedAuctionType,
      minBuyNow: "0",
      maxBuyNow: "0",
      minAuction: "0",
      maxAuction: "0"
    });
  }

  private getConditionFormGroup() {
  }

  ngOnInit() {
  }

  applyClick() {
    if (this.form.valid) {
      let filters = new AuctionFilters(this.form.value.condition, this.form.value.type, this.form.value.minBuyNow, this.form.value.maxBuyNow, this.form.value.minAuction, this.form.value.maxAuction);
      if(this.lastFilters && this.lastFilters.equals(filters)){
        //return;
      }
      this.lastFilters = filters;
      console.log(filters);

      this.applyFilters.emit(filters);
    }
  }

}
