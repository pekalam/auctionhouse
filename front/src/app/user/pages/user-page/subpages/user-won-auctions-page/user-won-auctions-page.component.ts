import { Component, OnInit } from '@angular/core';
import { Auction } from 'src/app/core/models/Auctions';
import { UserAuctionsSortDir, UserAuctionsSorting } from 'src/app/core/queries/UserAuctionsQuery';
import { UserWonAuctionsQuery } from 'src/app/core/queries/UserWonAuctionsQuery';
import { PageEvent } from '@angular/material';

@Component({
  selector: 'app-user-won-auctions-page',
  templateUrl: './user-won-auctions-page.component.html',
  styleUrls: ['./user-won-auctions-page.component.scss']
})
export class UserWonAuctionsPageComponent implements OnInit {

  auctions: Auction[] = [];
  total = 0;
  currentPage = 0;
  descending = UserAuctionsSortDir.DESCENDING;
  sorting = UserAuctionsSorting.DATE_CREATED;

  constructor(private userWonAuctionsQuery: UserWonAuctionsQuery) {
    this.fetchAuctions();
  }

  ngOnInit() {
  }

  private fetchAuctions() {
    this.userWonAuctionsQuery.execute(this.currentPage, this.sorting, this.descending).subscribe((result) => {
      if (result) {
        this.auctions = result.auctions;
      }
    });
  }

  onPageChange(ev: PageEvent) {
    this.currentPage = ev.pageIndex;
    this.fetchAuctions();
  }

  onDirChange(descending: UserAuctionsSortDir) {
    this.descending = descending;
    this.fetchAuctions();
  }

  onSortChange(value: UserAuctionsSorting) {
    this.sorting = value;
    this.fetchAuctions();
  }

}
