import { Component, OnInit, Input, Output, EventEmitter, OnDestroy } from '@angular/core';
import { Auction } from '../../../core/models/Auctions';
import { Router } from '@angular/router';

@Component({
  selector: 'app-auction',
  templateUrl: './auction.component.html',
  styleUrls: ['./auction.component.scss']
})
export class AuctionComponent implements OnInit, OnDestroy {
  private timeoutHandle;

  @Input('auction')
  set setAuction(auction: Auction) {
    if (auction) {
      this.auction = auction;
      this.calculateAuctionTime();
      this.clearAuctionTimeCalcInterval();
      this.timeoutHandle = setInterval(() => { this.calculateAuctionTime(); });
    }
  }
  @Output() buyNow = new EventEmitter<Auction>();
  @Output() bid = new EventEmitter<Auction>();
  @Output() auctionTimeout = new EventEmitter<Auction>();

  showTimer = false;
  timer = { m: '', s: '' };
  auction: Auction;

  constructor() {
  }

  ngOnInit() {
  }

  ngOnDestroy(): void {
    this.clearAuctionTimeCalcInterval();
  }

  private clearAuctionTimeCalcInterval() {
    if (this.timeoutHandle) {
      clearTimeout(this.timeoutHandle);
    }
  }

  private updateTimer() {
    let d1 = new Date(this.auction.endDate);
    let d2 = new Date();
    let minutesLeft = Math.round((d1.getTime() - d2.getTime()) / (1000 * 60));
    let secondsLeft = Math.abs(Math.round((d1.getTime() - d2.getTime()) / 1000) - minutesLeft * 60);
    if (minutesLeft < 0) {
      clearTimeout(this.timeoutHandle);
      this.auctionTimeout.emit(this.auction);
    }
    this.timer.m = minutesLeft.toString();
    this.timer.s = secondsLeft < 10 ? `0${secondsLeft}` : secondsLeft.toString();
  }

  private calculateAuctionTime() {
    if (this.showTimer) {
      this.updateTimer();
    } else {
      let d1 = new Date(this.auction.endDate);
      let d2 = new Date(this.auction.startDate);
      let minutesLeft = Math.round((d1.getTime() - d2.getTime()) / (1000 * 60));
      if (minutesLeft <= 10) {
        this.showTimer = true;
        this.updateTimer();
      }
    }
  }

  getDaysLeft(): number {
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
