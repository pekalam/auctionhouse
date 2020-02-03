import { Component, OnInit, OnDestroy } from '@angular/core';
import { EndingAuctionsQuery, EndingAuctionsQueryResult } from '../../../core/queries/EndingAuctionsQuery';
import { AuctionImageQuery } from '../../../core/queries/AuctionImageQuery';


export interface EndingAuction {
  queryResult: EndingAuctionsQueryResult;
  timeLeft: Date;
}

@Component({
  selector: 'app-ending-auctions',
  templateUrl: './ending-auctions.component.html',
  styleUrls: ['./ending-auctions.component.scss']
})
export class EndingAuctionsComponent implements OnInit, OnDestroy {

  endingAuctions: EndingAuction[];
  imgs: string[] = [];

  private timerInterval;

  constructor(private endingAuctionsQuery: EndingAuctionsQuery,
    private auctionImageQuery: AuctionImageQuery) { }


  private calcTimeLeft(endDate: Date): Date {
    return new Date(endDate.getTime() - new Date().getTime());
  }

  private updateTimers() {
    for (const auction of this.endingAuctions) {
      auction.timeLeft = this.calcTimeLeft(new Date(auction.queryResult.endDate));
    }
  }

  ngOnDestroy(): void {
    clearInterval(this.timerInterval);
  }

  ngOnInit() {
    this.endingAuctionsQuery.execute().subscribe((r) => {
      this.imgs = r.map((a) => this.auctionImageQuery.execute(a.auctionImages[0].size3Id));
      this.endingAuctions = r.map((a) => {
        return {
          queryResult: a,
          timeLeft: this.calcTimeLeft(new Date(a.endDate))
        } as EndingAuction;
      });
      this.timerInterval = setInterval(() => this.updateTimers(), 1000);
    });
  }

}
