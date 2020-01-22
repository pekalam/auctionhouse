import { Component, OnInit } from '@angular/core';
import { BreakpointObserver, BreakpointState } from '@angular/cdk/layout';
import { MostViewedAuctionsQuery, MostViewedAuction } from '../../../core/queries/MostViewedAuctionsQuery';
import { Auction } from '../../../core/models/Auctions';
import { RecentlyViewedService } from '../../../core/services/RecentlyViewedService';
import { AuthenticationStateService } from '../../../core/services/AuthenticationStateService';
import { AuctionImageQuery } from '../../../core/queries/AuctionImageQuery';

@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.component.html',
  styleUrls: ['./home-page.component.scss']
})
export class HomePageComponent implements OnInit {

  mostViewedAuctions: MostViewedAuction[];

  constructor(private mostViewedAuctionsQuery: MostViewedAuctionsQuery) {
    this.mostViewedAuctionsQuery.execute().subscribe((result) => {
      this.mostViewedAuctions = result;
    });
  }

  ngOnInit() {

  }

}
