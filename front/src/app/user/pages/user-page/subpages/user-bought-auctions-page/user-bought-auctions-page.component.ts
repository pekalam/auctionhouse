import { Component, OnInit } from '@angular/core';
import { UserBoughtAuctionsQuery, UserBoughtAuctions } from 'src/app/core/queries/UserBoughtAuctionsQuery';
import { Auction } from '../../../../../core/models/Auctions';
import { PageEvent } from '@angular/material';

@Component({
  selector: 'app-user-bought-auctions-page',
  templateUrl: './user-bought-auctions-page.component.html',
  styleUrls: ['./user-bought-auctions-page.component.scss']
})
export class UserBoughtAuctionsPageComponent implements OnInit {

  auctions: Auction[] = [];
  total = 0;
  currentPage = 0;

  constructor(private userBoughtAuctionsQuery: UserBoughtAuctionsQuery) {
    this.fetchUserBoughtAuctions();
  }

  ngOnInit() {
  }

  private fetchUserBoughtAuctions() {
    this.userBoughtAuctionsQuery.execute(this.currentPage).subscribe((result) => {
      if (result) {
        this.auctions = result.auctions;
      }
    });
  }

  onPageChange(ev: PageEvent) {
    this.currentPage = ev.pageIndex;
    this.fetchUserBoughtAuctions();
  }

}
