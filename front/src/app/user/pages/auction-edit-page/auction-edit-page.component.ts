import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Auction } from 'src/app/core/models/Auctions';
import { FormGroup, FormControl, Validators, ValidatorFn, AbstractControl } from '@angular/forms';

@Component({
  selector: 'app-auction-edit-page',
  templateUrl: './auction-edit-page.component.html',
  styleUrls: ['./auction-edit-page.component.scss']
})
export class AuctionEditPageComponent implements OnInit {


  auction: Auction;

  constructor(private router: Router) {
    if (this.router.getCurrentNavigation().extras.state) {
      this.auction = this.router.getCurrentNavigation().extras.state.auction;
    }else{
      this.router.navigate(['/user']);
    }
  }

  ngOnInit() {
  }

}
