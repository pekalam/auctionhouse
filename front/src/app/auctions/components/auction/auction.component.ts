import { Component, OnInit, Input, Output, EventEmitter, OnDestroy } from '@angular/core';
import { Auction } from '../../../core/models/Auctions';
import { Router } from '@angular/router';
import { AuctionImageQuery } from 'src/app/core/queries/AuctionImageQuery';
import { AuthenticationStateService } from '../../../core/services/AuthenticationStateService';
import { UserIdentity } from '../../../core/models/UserIdentity';

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
      this.imgs = auction.auctionImages.filter(img => img != null).map(img => this.auctionImageQuery.execute(img.size1Id));
      this.setDaysLeft();
      this.calculateAuctionTime();
      this.clearAuctionTimeCalcInterval();
      this.timeoutHandle = setInterval(() => { this.calculateAuctionTime(); });
    }
    console.log("set");

  }
  @Input()
  showAuctionButtons: boolean = false;
  @Output() buyNow = new EventEmitter<Auction>();
  @Output() bid = new EventEmitter<Auction>();
  @Output() auctionTimeout = new EventEmitter<Auction>();

  showTimer = true;
  timer = { m: '', s: '' };
  auction: Auction;
  daysLeft = 0;
  imgs = [];
  currentUser: UserIdentity = null;

  private setDaysLeft(){
    let d1 = new Date(this.auction.endDate);
    let d2 = new Date(this.auction.startDate);
    this.daysLeft = Math.round((d1.getTime() - d2.getTime()) / (1000 * 60 * 60 * 24));
  }

  constructor(private auctionImageQuery: AuctionImageQuery, public authenticationStateService: AuthenticationStateService) {
    authenticationStateService.currentUser.subscribe((u) => this.currentUser = u);
    authenticationStateService.checkIsAuthenticated();
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


  onBuyNow() {
    this.buyNow.emit(this.auction);
  }

  onBid() {
    this.bid.emit(this.auction);
  }
}
