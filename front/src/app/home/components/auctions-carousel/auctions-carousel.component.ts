import { Component, OnInit, Input } from '@angular/core';
import { AuctionListModel } from '../../../core/models/Auctions';
import { MostViewedAuction } from '../../../core/queries/MostViewedAuctionsQuery';
import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { AuctionImageQuery } from '../../../core/queries/AuctionImageQuery';

@Component({
  selector: 'app-auctions-carousel',
  templateUrl: './auctions-carousel.component.html',
  styleUrls: ['./auctions-carousel.component.scss']
})
export class AuctionsCarouselComponent implements OnInit {

  auctions: MostViewedAuction[] = [];
  imgSources: string[] = [];

  @Input('auctions')
  set setAuctions(auctions: MostViewedAuction[]) {
    if (!auctions || auctions.length <= 0) { return; }

    this.imgSources = auctions
      .map(a => a.auctionImages[0] ? this.getImageUrl(a.auctionImages[0].size1Id) : null)
      .filter(i => i != null);
    this.auctions = auctions;
  }
  currentAuction = 0;

  imgHeight;

  constructor(public breakpointObserver: BreakpointObserver, private auctionImageQuery: AuctionImageQuery) {
    this.breakpointObserver
      .observe(['(max-width: 820px)'])
      .subscribe((state: BreakpointState) => {
        if (state.matches) {
          this.imgHeight = 133;
        } else {
          this.imgHeight = 200;
        }
      });

    this.breakpointObserver
      .observe(['(max-width: 1100px)'])
      .subscribe((state: BreakpointState) => {
        if (state.matches) {
          this.imgHeight = 150;
        } else {
          this.imgHeight = 200;
        }
      });
  }

  private getImageUrl(imageId: string): string{
    return this.auctionImageQuery.execute(imageId);
  }

  nextAuction(ev : Event) {
    ev.stopPropagation();
    if (this.currentAuction + 1 < this.auctions.length) {
      this.currentAuction++;
      this.imgSources = [this.getImageUrl(this.auctions[this.currentAuction].auctionImages[0].size1Id)];
      document.getElementById('auction-container').style.cssText = '';
      window.requestAnimationFrame(() => {
        document.getElementById('auction-container').style.cssText = 'animation: fadeIn 0.5s ease 0s 1 normal forwards running;';
      });
    }
  }

  prevAuction(ev: Event) {
    ev.stopPropagation();
    if (this.currentAuction - 1 >= 0) {
      this.currentAuction--;
      this.imgSources = [this.getImageUrl(this.auctions[this.currentAuction].auctionImages[0].size1Id)];
      document.getElementById('auction-container').style.cssText = '';
      window.requestAnimationFrame(() => {
        document.getElementById('auction-container').style.cssText = 'animation: fadeIn 0.5s ease 0s 1 normal forwards running;';
      });
    }
  }

  ngOnInit() {
  }
}
