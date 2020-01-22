import { Component, OnInit } from '@angular/core';
import { Auction } from 'src/app/core/models/Auctions';
import { RecentlyViewedService } from 'src/app/core/services/RecentlyViewedService';
import { AuctionImageQuery } from 'src/app/core/queries/AuctionImageQuery';

@Component({
  selector: 'app-recently-viewed',
  templateUrl: './recently-viewed.component.html',
  styleUrls: ['./recently-viewed.component.scss']
})
export class RecentlyViewedComponent implements OnInit {

  recentlyViewed: Auction[];
  imgs: string[];

  constructor(private recentlyViewedService: RecentlyViewedService, private auctionImageQuery: AuctionImageQuery) { }

  ngOnInit() {
    this.recentlyViewedService.getRecentlyViewedAuctions().subscribe((auctions) => {
      this.imgs = auctions.map((a) => this.auctionImageQuery.execute(a.auctionImages[0].size3Id));
      this.recentlyViewed = auctions;
    });
  }

}
