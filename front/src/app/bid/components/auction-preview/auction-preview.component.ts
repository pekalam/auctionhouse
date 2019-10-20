import { Component, OnInit, Input } from '@angular/core';
import { Auction } from '../../../core/models/Auctions';

@Component({
  selector: 'app-auction-preview',
  templateUrl: './auction-preview.component.html',
  styleUrls: ['./auction-preview.component.scss']
})
export class AuctionPreviewComponent implements OnInit {

  @Input() auction: Auction;

  constructor() { }

  ngOnInit() {
  }

}
