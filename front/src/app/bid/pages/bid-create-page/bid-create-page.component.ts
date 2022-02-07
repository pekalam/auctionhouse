import { Component, OnInit, OnDestroy } from '@angular/core';
import { Auction } from '../../../core/models/Auctions';
import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { RaiseBidCommand } from "../../../core/commands/auction/RaiseBidCommand";
import { RequestStatus, WSCommandStatusService } from 'src/app/core/services/WSCommandStatusService';
import { Subscription } from 'rxjs';
import { first, take } from 'rxjs/operators';
import { AuctionPriceChangedNotificationName, AuctionPriceChangedNotification } from '../../../core/serverNotifications/AuctionPriceChangedNotification';
import { Bid } from '../../../core/models/Bid';
import { AuthenticationStateService } from 'src/app/core/services/AuthenticationStateService';


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
  disableSubmitButton: boolean = false;

  constructor(private activatedRoute: ActivatedRoute, private raiseBidCommand: RaiseBidCommand, private router: Router, private wsCommandStatusService: WSCommandStatusService, private authenticationStateService: AuthenticationStateService) {
    this.activatedRoute.data.subscribe(async (data: {auction: Auction}) => {
      this.auction = data.auction;
      this.form.controls.price.setValue(this.auction.winningBid ? (Number.parseFloat(this.auction.winningBid.price) + 1).toString() : '');
      await this.updateSubmitButton();
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

  private async updateSubmitButton(){
    if(!this.auction.winningBid){
      return;
    }
    const currentUser = await this.authenticationStateService.currentUser.pipe(take(1)).toPromise();
    this.disableSubmitButton = this.auction.winningBid && currentUser.userId == this.auction.winningBid.userIdentity.userId;
  }

  async onAuctionPriceChangedNotification(values: AuctionPriceChangedNotification) {
    console.log(`New price: ${values.newPrice}`);
    this.auction.winningBid = {
      auctionId: this.auction.auctionId,
      bidId: values.bidId,
      dateCreated: values.dateCreated,
      price: values.newPrice,
      userIdentity: {
        userId: values.winnerId,
        userName: null, //TODO
      },
    }

    window.requestAnimationFrame(function () {
      document.getElementById('auction-price-box').style.cssText = '';
      window.requestAnimationFrame(function () {
        document.getElementById('auction-price-box').style.cssText =
          'animation: auction-price-change-box 1s linear 0s 1 normal both running;';
      })
    })

    await this.updateSubmitButton();
  }

  onBidSubmit() {
    if (this.form.valid) {

      this.raiseBidCommand
        .execute(this.auction.auctionId, this.form.value.price)
        .subscribe((msg: RequestStatus) => {
          if (msg.status === 'COMPLETED') {
            //this.router.navigate(['/auction'], { queryParams: { auctionId: this.auction.auctionId } });
          } else {
            console.log('error ' + msg);
            this.router.navigate(['/error']);
          }
        });
    }
  }

}
