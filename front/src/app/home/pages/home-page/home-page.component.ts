import { Component, OnInit } from '@angular/core';
import { BreakpointObserver, BreakpointState } from '@angular/cdk/layout';
import { MostViewedAuctionsQuery, MostViewedAuction } from '../../../core/queries/MostViewedAuctionsQuery';

@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.component.html',
  styleUrls: ['./home-page.component.scss']
})
export class HomePageComponent implements OnInit {

  mostViewedAuctions: MostViewedAuction[];

  constructor(private mostViewedAuctionsQuery: MostViewedAuctionsQuery) {
    mostViewedAuctionsQuery.execute().subscribe((result) => {
      this.mostViewedAuctions = result;
    });
  }

  ngOnInit() {

  }

}
