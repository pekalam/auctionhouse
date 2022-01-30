import { Component, OnInit, OnDestroy } from '@angular/core';
import { Auction } from '../../../core/models/Auctions';
import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { RaiseBidCommand } from "../../../core/commands/auction/RaiseBidCommand";
import { RequestStatus, WSCommandStatusService } from 'src/app/core/services/WSCommandStatusService';
import { Subscription } from 'rxjs';
import { first } from 'rxjs/operators';
import { AuctionPriceChangedNotificationName, AuctionPriceChangedNotification } from '../../../core/serverNotifications/AuctionPriceChangedNotification';
import { Bid } from '../../../core/models/Bid';


@Component({
  selector: 'app-bid-create-page',
  templateUrl: './bid-create-page.component.html',
  styleUrls: ['./bid-create-page.component.scss']
})
export class BidCreatePageComponent implements OnInit, OnDestroy {
  auction: Auction;
  form = new FormGroup({
    price: new FormControl('', [Validators.required])
  });

  constructor(private activatedRoute: ActivatedRoute, private raiseBidCommand: RaiseBidCommand, private router: Router, private wsCommandStatusService: WSCommandStatusService) {
    this.activatedRoute.data.subscribe((data) => {
      this.auction = data.auction;
      this.form.controls.price.setValue(this.auction.winningBid ? (Number.parseFloat(this.auction.winningBid.price) + 1).toString() : '');
    });
    this.wsCommandStatusService
      .setupServerNotificationHandler<AuctionPriceChangedNotification>(AuctionPriceChangedNotificationName(this.auction.auctionId))
      .subscribe((v) => this.onAuctionPriceChangedNotification(v));
  }

  ngOnInit() {
  }

  ngOnDestroy(): void {
    this.wsCommandStatusService.deleteServerNotificationHandler(AuctionPriceChangedNotificationName(this.auction.auctionId));
    this.wsCommandStatusService.closeConnection();
  }

  onAuctionPriceChangedNotification(values: AuctionPriceChangedNotification) {
    console.log(`New price: ${values.newPrice}`);
    this.auction.winningBid.price = values.newPrice;
    this.auction.winningBid.bidId = values.bidId;
    this.auction.winningBid.userIdentity.userId = values.winnerId; //TODO name
    this.auction.winningBid.dateCreated = values.dateCreated; //TODO name

    window.requestAnimationFrame(function () {
      document.getElementById('auction-price-box').style.cssText = '';
      window.requestAnimationFrame(function () {
        document.getElementById('auction-price-box').style.cssText =
          'animation: auction-price-change-box 1s linear 0s 1 normal both running;';
      })
    })

  }

  onBidSubmit() {
    if (this.form.valid) {

      this.raiseBidCommand
        .execute(this.auction.auctionId, this.form.value.price)
        .subscribe((msg: RequestStatus) => {
          if (msg.status === 'COMPLETED') {
            this.router.navigate(['/auction'], { queryParams: { auctionId: this.auction.auctionId } });
          } else {
            console.log('error ' + msg);
            this.router.navigate(['/error']);
          }
        });
    }
  }

}
