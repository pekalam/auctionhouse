import { Component, OnInit } from '@angular/core';
import { UserBoughtAuctionsQuery, UserBoughtAuctions } from 'src/app/core/queries/UserBoughtAuctionsQuery';
import { Auction } from '../../../../../core/models/Auctions';
import { PageEvent } from '@angular/material';
import { UserAuctionsSortDir, UserAuctionsSorting } from 'src/app/core/queries/UserAuctionsQuery';

@Component({
  selector: 'app-user-bought-auctions-page',
  templateUrl: './user-bought-auctions-page.component.html',
  styleUrls: ['./user-bought-auctions-page.component.scss']
})
export class UserBoughtAuctionsPageComponent implements OnInit {

  auctions: Auction[] = [];
  total = 0;
  currentPage = 0;
  descending = UserAuctionsSortDir.DESCENDING;
  sorting = UserAuctionsSorting.DATE_CREATED;

  constructor(private userBoughtAuctionsQuery: UserBoughtAuctionsQuery) {
    this.fetchAuctions();
  }

  ngOnInit() {
  }

  private fetchAuctions() {
    this.userBoughtAuctionsQuery.execute(this.currentPage, this.sorting, this.descending).subscribe((result) => {
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
