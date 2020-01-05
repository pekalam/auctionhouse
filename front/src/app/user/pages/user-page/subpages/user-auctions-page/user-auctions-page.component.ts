import { Component, OnInit } from '@angular/core';
import { UserAuctions, UserAuctionsQuery, UserAuctionsSorting, UserAuctionsSortDir } from '../../../../../core/queries/UserAuctionsQuery';
import { Router } from '@angular/router';
import { Auction } from 'src/app/core/models/Auctions';
import { MatSlideToggleChange } from '@angular/material';
import { pipe } from 'rxjs';

@Component({
  selector: 'app-user-auctions-page',
  templateUrl: './user-auctions-page.component.html',
  styleUrls: ['./user-auctions-page.component.scss']
})
export class UserAuctionsPageComponent implements OnInit {

  auctions: Auction[];
  currentPage = 0;
  total = 0;
  showArchived = false;
  descending = UserAuctionsSortDir.DESCENDING;
  sorting = UserAuctionsSorting.DATE_CREATED;

  constructor(private userAuctionsQuery: UserAuctionsQuery, private router: Router) {
    this.fetchAuctions();
  }

  ngOnInit() {
  }

  private fetchAuctions() {
    this.userAuctionsQuery
      .execute(this.currentPage, this.showArchived, this.sorting, this.descending)
      .subscribe((v) => {
        this.auctions = v.auctions;
        this.total = v.total;
      });
  }

  onAuctionClick(clickedAuction: Auction) {
    this.router.navigate(['/editAuction'], { state: { auction: clickedAuction } })
  }

  onPageChange(newPage: number) {
    this.currentPage = newPage;
    this.fetchAuctions();
  }

  onShowArchived(change: MatSlideToggleChange) {
    this.showArchived = change.checked;
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
