import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { AuctionListModel } from '../../../core/models/Auctions';
import { MostViewedAuction } from '../../../core/queries/MostViewedAuctionsQuery';
import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { AuctionImageQuery } from '../../../core/queries/AuctionImageQuery';

@Component({
  selector: 'app-auctions-carousel',
  templateUrl: './auctions-carousel.component.html',
  styleUrls: ['./auctions-carousel.component.scss']
})
export class AuctionsCarouselComponent implements OnInit, OnDestroy {
  private rotateInterval;
  private animationTimeout;

  auctions: MostViewedAuction[] = [];
  imgSources: string[] = [];
  currentAuction = 0;
  imgHeight;

  @Input('rotateTime')
  set setRotateTime(rotateTime: number) {
    if (this.rotateInterval) {
      clearInterval(this.rotateInterval);
    }
    if (rotateTime > 0) {
      this.rotateInterval = setInterval(() => this.rotate(), rotateTime);
    }
  }

  @Input('auctions')
  set setAuctions(auctions: MostViewedAuction[]) {
    if (!auctions || auctions.length <= 0) { return; }

    this.imgSources = auctions
      .map(a => a.auctionImages[0] ? this.getImageUrl(a.auctionImages[0].size1Id) : null)
      .filter(i => i != null);
    this.auctions = auctions;
  }

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

  ngOnDestroy(): void {
    clearInterval(this.rotateInterval);
    clearTimeout(this.animationTimeout);
  }

  private rotate() {
    const previousAuction = this.currentAuction;
    this.currentAuction = (this.currentAuction + 1) % this.auctions.length;
    if(!Number.isFinite(this.currentAuction)){
      return;
    }
    if(previousAuction == this.currentAuction) { 
      return;
    }
    this.changeAuction(1000);
  }

  private getImageUrl(imageId: string): string {
    return this.auctionImageQuery.execute(imageId);
  }

  private changeAuction(animSec: number = 500) {
    this.imgSources = [this.getImageUrl(this.auctions[this.currentAuction].auctionImages[0].size1Id)];
    clearTimeout(this.animationTimeout);
    document.getElementById('auction-container').style.cssText = '';
    window.requestAnimationFrame(() => {
      document.getElementById('auction-container')
        .style.cssText = `animation: fadeIn ${animSec / 1000}s ease 0s 1 normal forwards running;`;
      this.animationTimeout = setTimeout(() => {
        document.getElementById('auction-container').style.cssText = '';
      }, animSec);
    });
  }

  nextAuction(ev: Event) {
    ev.stopPropagation();
    clearInterval(this.rotateInterval);
    if (this.currentAuction + 1 < this.auctions.length) {
      this.currentAuction++;
      this.changeAuction();
    }
  }

  prevAuction(ev: Event) {
    ev.stopPropagation();
    clearInterval(this.rotateInterval);
    if (this.currentAuction - 1 >= 0) {
      this.currentAuction--;
      this.changeAuction();
    }
  }

  ngOnInit() {
  }
}
