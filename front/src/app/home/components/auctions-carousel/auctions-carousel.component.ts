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
    if(!auctions || auctions.length <= 0){return;}
    this.imgSources = [`/api/auctionImage?img=${auctions[0].auctionImages[0].size1Id}`];
    this.auctions = auctions;
  }
  currentAuction = 0;

  imgHeight;

  constructor(public breakpointObserver: BreakpointObserver) {
    this.breakpointObserver
      .observe(['(max-width: 820px)'])
      .subscribe((state: BreakpointState) => {
        if (state.matches) {
          console.log('asd');

          this.imgHeight = 133;
        } else {
          this.imgHeight = 200;
        }
      });

    this.breakpointObserver
      .observe(['(max-width: 1100px)'])
      .subscribe((state: BreakpointState) => {
        if (state.matches) {
          console.log('asd');

          this.imgHeight = 150;
        } else {
          this.imgHeight = 200;
        }
      });
  }

  nextAuction() {
    if (this.currentAuction + 1 < this.auctions.length) {
      this.currentAuction++;
      this.imgSources = [`/api/auctionImage?img=${this.auctions[this.currentAuction].auctionImages[0].size1Id}`];
      document.getElementById('auction-container').style.cssText = '';
      window.requestAnimationFrame(function (){
        document.getElementById('auction-container').style.cssText = 'animation: fadeIn 0.5s ease 0s 1 normal forwards running;';
      });
    }
  }

  prevAuction() {
    if (this.currentAuction - 1 >= 0) {
      this.currentAuction--;
      this.imgSources = [`/api/auctionImage?img=${this.auctions[this.currentAuction].auctionImages[0].size1Id}`];
      document.getElementById('auction-container').style.cssText = '';
      window.requestAnimationFrame(function (){
        document.getElementById('auction-container').style.cssText = 'animation: fadeIn 0.5s ease 0s 1 normal forwards running;';
      });
    }
  }

  ngOnInit() {
  }
}
