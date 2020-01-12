import { Component, OnInit, Input } from '@angular/core';
import { Auction, AuctionListModel } from '../../../core/models/Auctions';
import { Router } from '@angular/router';
import { AuctionImageQuery } from '../../../core/queries/AuctionImageQuery';

@Component({
  selector: 'app-auction-list-item',
  templateUrl: './auction-list-item.component.html',
  styleUrls: ['./auction-list-item.component.scss']
})
export class AuctionListItemComponent implements OnInit {

  @Input() auction: AuctionListModel;
  auctionImgSrc;

  constructor(private auctionImageQuery: AuctionImageQuery , private router: Router) { }

  ngOnInit() {
    if(this.auction.auctionImages[0]){
      this.auctionImgSrc = this.auctionImageQuery.execute(this.auction.auctionImages[0].size2Id);

    }
  }

  onAuctionListItemClick() {
    this.router.navigate(['/auction'], {
      queryParams: {
        auctionId: this.auction.auctionId
      }
    });
  }
}
