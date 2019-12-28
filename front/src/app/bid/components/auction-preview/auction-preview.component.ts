import { Component, OnInit, Input } from '@angular/core';
import { Auction } from '../../../core/models/Auctions';
import { AuctionImageQuery } from 'src/app/core/queries/AuctionImageQuery';
import { ThrowStmt } from '@angular/compiler';

@Component({
  selector: 'app-auction-preview',
  templateUrl: './auction-preview.component.html',
  styleUrls: ['./auction-preview.component.scss']
})
export class AuctionPreviewComponent implements OnInit {

  auction: Auction;
  imgSrc: string;

  @Input('auction')
  set setAuction(auction: Auction) {
    this.auction = auction;
    const frst = auction.auctionImages
    .filter(img => img != null)[0];
    this.imgSrc = this.auctionImageQuery.execute(frst.size1Id);
  }

  constructor(private auctionImageQuery: AuctionImageQuery) { }

  ngOnInit() {
  }

}
