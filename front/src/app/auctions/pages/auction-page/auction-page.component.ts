import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Auction } from '../../../core/models/Auctions';
import { Category } from '../../../core/models/Category';
import { AuctionQuery } from '../../../core/queries/AuctionQuery';
import { AuthenticationStateService } from '../../../core/services/AuthenticationStateService';
import { MatDialogRef, MatDialog } from '@angular/material';
import { AuctionBuynowDialogComponent } from './auction-buynow-dialog/auction-buynow-dialog.component';

@Component({
  selector: 'app-auction-page',
  templateUrl: './auction-page.component.html',
  styleUrls: ['./auction-page.component.scss']
})
export class AuctionPageComponent implements OnInit {

  auction: Auction;

  constructor(private activatedRoute: ActivatedRoute,
              private auctionQuery: AuctionQuery,
              private router: Router,
              private dialog: MatDialog) {
    this.activatedRoute.queryParams.subscribe((p) => {
      this.auctionQuery
        .execute(p.auctionId)
        .subscribe((v) => this.auction = v);
    });
  }

  ngOnInit() {
  }

  onBidClick() {
    this.router.navigate(['/createbid'], { queryParams: { auctionId: this.auction.auctionId } });
  }

  onBuyNowClick() {
    this.dialog.open(AuctionBuynowDialogComponent, {
      data: { auction: this.auction }
    });
  }

  onAuctionTimeout() {
    this.router.navigate(['/home']);
  }

}
