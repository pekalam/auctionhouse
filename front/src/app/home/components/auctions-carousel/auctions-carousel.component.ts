import { Component, OnInit, Input } from '@angular/core';
import { AuctionListModel } from '../../../core/models/Auctions';
import { MostViewedAuction } from '../../../core/queries/MostViewedAuctionsQuery';
import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';

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
    this.imgSources = auctions.map((a) => `/api/auctionImage?img=${a.auctionImages[0].size1Id}`);
    this.auctions = auctions;
  }
  selectedImg = 0;

  imgHeight;

  constructor(public breakpointObserver: BreakpointObserver) {
    this.breakpointObserver
      .observe(['(max-width: 820px)'])
      .subscribe((state: BreakpointState) => {
        if (state.matches) {
          console.log("asd");

          this.imgHeight = 133;
        } else {
          this.imgHeight = 200;
        }
      });

    this.breakpointObserver
      .observe(['(max-width: 1100px)'])
      .subscribe((state: BreakpointState) => {
        if (state.matches) {
          console.log("asd");

          this.imgHeight = 150;
        } else {
          this.imgHeight = 200;
        }
      });
  }

  ngOnInit() {
  }
}
