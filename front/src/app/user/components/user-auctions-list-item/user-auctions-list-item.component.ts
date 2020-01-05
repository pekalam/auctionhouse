import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { Auction } from '../../../core/models/Auctions';
import { AuctionImageQuery } from '../../../core/queries/AuctionImageQuery';

@Component({
  selector: 'app-user-auctions-list-item',
  templateUrl: './user-auctions-list-item.component.html',
  styleUrls: ['./user-auctions-list-item.component.scss']
})
export class UserAuctionsListItemComponent implements OnInit {

  @Input()
  auction: Auction;

  @Output()
  itemClick = new EventEmitter<Auction>();

  constructor(private auctionImageQuery: AuctionImageQuery) { }

  ngOnInit() {
  }

  getImageUrl(imageId: string): string{
    return this.auctionImageQuery.execute(imageId);
  }

}
