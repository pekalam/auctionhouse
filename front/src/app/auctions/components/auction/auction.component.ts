import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Auction } from '../../../core/models/Auctions';
import { Router } from '@angular/router';

@Component({
  selector: 'app-auction',
  templateUrl: './auction.component.html',
  styleUrls: ['./auction.component.scss']
})
export class AuctionComponent implements OnInit {

  @Input() auction: Auction;
  @Output() buyNow = new EventEmitter<Auction>();
  @Output() bid = new EventEmitter<Auction>();

  constructor() {

  }

  ngOnInit() {

  }

  getDaysLeft() : number{
    console.log(this.auction.endDate);
    let d1 = new Date(this.auction.endDate);
    let d2 = new Date(this.auction.startDate);

    return Math.round((d1.getTime() - d2.getTime()) / (1000 * 60 * 60 * 24));
  }



  onBuyNow() {
    this.buyNow.emit(this.auction);
  }

  onBid() {
    this.bid.emit(this.auction);
  }
}
