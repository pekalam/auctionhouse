import { Component, OnInit } from '@angular/core';
import { UserAuctions, UserAuctionsQuery } from '../../../../../core/queries/UserAuctionsQuery';
import { Router } from '@angular/router';
import { Auction } from 'src/app/core/models/Auctions';

@Component({
  selector: 'app-user-auctions-page',
  templateUrl: './user-auctions-page.component.html',
  styleUrls: ['./user-auctions-page.component.scss']
})
export class UserAuctionsPageComponent implements OnInit {

  auctions: Auction[];
  currentPage = 0;
  total = 0;

  constructor(private userAuctionsQuery: UserAuctionsQuery, private router: Router) {
    this.fetchAuctions();
  }

  ngOnInit() {
  }

  private fetchAuctions() {
    this.userAuctionsQuery.execute(this.currentPage).subscribe((v) => {
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
}
