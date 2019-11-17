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

  userAuctions: UserAuctions;

  constructor(private userAuctionsQuery: UserAuctionsQuery, private router: Router) {
    userAuctionsQuery.execute(0).subscribe((auctions) => {
      this.userAuctions = auctions;
    })
   }

  ngOnInit() {
  }


  onAuctionClick(clickedAuction: Auction){
    this.router.navigate(['/editAuction'] , {state: {auction: clickedAuction}})
  }
}
