import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Auction } from '../../../core/models/Auctions';
import { Category } from '../../../core/models/Category';
import { AuctionQuery } from '../../../core/queries/AuctionQuery';
import { AuthenticationStateService } from '../../../core/services/AuthenticationStateService';

@Component({
  selector: 'app-auction-page',
  templateUrl: './auction-page.component.html',
  styleUrls: ['./auction-page.component.scss']
})
export class AuctionPageComponent implements OnInit {

  auction: Auction;
  showAuctionButtons = false;

  constructor(private activatedRoute: ActivatedRoute,
    private auctionQuery: AuctionQuery,
    private router: Router,
    private authenticationStateService: AuthenticationStateService) {
    this.activatedRoute.queryParams.subscribe((p) => {
      this.auctionQuery
        .execute(p.auctionId)
        .subscribe((v) => { this.setAuction(v); });
    });
  }

  private setAuction(auction: Auction) {
    this.authenticationStateService.currentUser.subscribe((user) => {
      console.log("current user: " + user);
      this.auction = auction;
      if (!user || user.userId !== auction.creator.userId) {
        this.showAuctionButtons = true;
      }
    })
  }

  ngOnInit() {
  }

  onBidClick() {
    this.router.navigate(['/createbid'], { queryParams: { auctionId: this.auction.auctionId } });
  }

  onBuyNowClick() {

  }

  onAuctionTimeout() {
    this.router.navigate(['/home']);
  }

}
