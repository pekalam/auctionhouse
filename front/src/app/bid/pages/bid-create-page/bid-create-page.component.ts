import { Component, OnInit, OnDestroy } from '@angular/core';
import { Auction } from '../../../core/models/Auctions';
import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { BidCommand } from '../../../core/commands/BidCommand';
import { RequestStatus, WSCommandStatusService } from 'src/app/core/services/WSCommandStatusService';
import { Subscription } from 'rxjs';
import { first } from 'rxjs/operators';
import { AuctionPriceChangedNotification_Name, AuctionPriceChangedNotification } from '../../../core/serverNotifications/AuctionPriceChangedNotification';

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

  constructor(private activatedRoute: ActivatedRoute, private bidCommand: BidCommand, private router: Router, private wsCommandStatusService: WSCommandStatusService) {
    this.activatedRoute.data.subscribe((data) => {
      this.auction = data.auction;
      this.form.controls.price.setValue(this.auction.winningBid ? this.auction.winningBid.price + 1 : '');
    });
    this.wsCommandStatusService
      .setupServerNotificationHandler<AuctionPriceChangedNotification>(AuctionPriceChangedNotification_Name)
      .subscribe((v) => this.onAuctionPriceChangedNotification(v));
  }

  ngOnInit() {
  }

  ngOnDestroy(): void {
    this.wsCommandStatusService.deleteServerNotificationHandler(AuctionPriceChangedNotification_Name);
  }

  onAuctionPriceChangedNotification(values: AuctionPriceChangedNotification) {
    console.log(`New price: ${values.winningBid.price}`);
    this.auction.winningBid = values.winningBid;
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

      this.bidCommand
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
