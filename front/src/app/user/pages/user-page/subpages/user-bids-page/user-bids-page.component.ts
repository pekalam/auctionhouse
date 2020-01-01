import { Component, OnInit, ViewChild } from '@angular/core';
import { UserBidsQueryResult, UserBidsQuery } from '../../../../../core/queries/UserBidsQuery';
import { UserBid } from '../../../../../core/models/UserBid';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { CancelBidCommand } from '../../../../../core/commands/auction/CancelBidCommand';
import { PageEvent } from '@angular/material';

interface UserBidVM extends UserBid {
  canBeCanceled: boolean;
}

@Component({
  selector: 'app-user-bids-page',
  templateUrl: './user-bids-page.component.html',
  styleUrls: ['./user-bids-page.component.scss']
})
export class UserBidsPageComponent implements OnInit {

  userBids: UserBidVM[];
  displayedColumns = ['auctionName', 'price', 'dateCreated', 'state']
  @ViewChild(MatSort, { static: false }) sort: MatSort;
  dataSource;

  total = 0;
  currentPage = 0;

  constructor(private userBidsQuery: UserBidsQuery, private cancelBidCommand: CancelBidCommand) {
    this.fetchBids();
  }

  private fetchBids() {
    this.userBidsQuery.execute(this.currentPage).subscribe((v) => {
      if (!v || !v.userBids) { return; }
      this.userBids = v.userBids.map(v => v as UserBidVM);
      this.total = v.total;
      for (const bid of this.userBids) {
        bid.canBeCanceled = this.canBeCanceled(bid);
      }
      this.dataSource = new MatTableDataSource(this.userBids);
      this.dataSource.sort = this.sort;
    });
  }

  //TODO: domain model
  private canBeCanceled(bid: UserBidVM): boolean {
    let now = new Date();
    console.log('c');

    now.setTime(now.getTime() - new Date(bid.dateCreated).getTime());

    return now.getMinutes() < 10;
  }

  onCancelBidClick(bid: UserBid) {
    this.cancelBidCommand.execute(bid.auctionId, bid.bidId).subscribe(response => {
      if (response.status === 'COMPLETED') {
        bid.bidCanceled = true;
      } else {
        console.log('cancel bid error');
      }
    });
  }

  ngOnInit() {
  }

  onPageChange(pageEvent: PageEvent) {
    this.currentPage = pageEvent.pageIndex;
    this.fetchBids();
  }

}
