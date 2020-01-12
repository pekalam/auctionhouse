import { Component, OnInit, Inject } from '@angular/core';
import { Auction } from '../../../../core/models/Auctions';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { AuthenticationStateService } from '../../../../core/services/AuthenticationStateService';
import { AuctionImageQuery } from '../../../../core/queries/AuctionImageQuery';
import { Router } from '@angular/router';
import { BuyNowCommand } from '../../../../core/commands/auction/BuyNowCommand';

@Component({
  selector: 'app-auction-buynow-dialog',
  templateUrl: './auction-buynow-dialog.component.html',
  styleUrls: ['./auction-buynow-dialog.component.scss']
})
export class AuctionBuynowDialogComponent implements OnInit {

  auction: Auction;
  imgsrc: string;

  constructor(@Inject(MAT_DIALOG_DATA) data: any,
              private dialogRef: MatDialogRef<AuctionBuynowDialogComponent>,
              private auctionImageQuery: AuctionImageQuery,
              private buyNowCommand: BuyNowCommand,
              private router: Router) {
    this.auction = data.auction;

    this.imgsrc = auctionImageQuery.execute(this.auction.auctionImages[0].size2Id);
  }

  ngOnInit() {
  }

  onCancelClick() {
    this.dialogRef.close();
  }

  onPayClick() {
    this.buyNowCommand.execute(this.auction.auctionId).subscribe((response) => {
      if (response.status === 'COMPLETED') {
        this.dialogRef.close();
        this.router.navigateByUrl('/user');
      } else {
        this.dialogRef.close();
        this.router.navigateByUrl('/error');
      }
    });
  }

}
