import { Component, OnInit } from '@angular/core';
import { UserAuctions, UserAuctionsQuery } from '../../../../../core/queries/UserAuctionsQuery';

@Component({
  selector: 'app-user-auctions-page',
  templateUrl: './user-auctions-page.component.html',
  styleUrls: ['./user-auctions-page.component.scss']
})
export class UserAuctionsPageComponent implements OnInit {

  userAuctions: UserAuctions;

  constructor(private userAuctionsQuery: UserAuctionsQuery) {
    userAuctionsQuery.execute(0).subscribe((auctions) => {
      this.userAuctions = auctions;
    })
   }

  ngOnInit() {
  }

}
