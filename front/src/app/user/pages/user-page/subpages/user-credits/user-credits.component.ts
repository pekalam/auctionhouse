import { Component, OnInit } from '@angular/core';
import { UserDataQuery, UserData } from '../../../../../core/queries/UserDataQuery';
import { FormGroup, FormControl } from '@angular/forms';
import { BuyCreditsCommand } from '../../../../../core/commands/user/BuyCreditsCommand';
import { Router } from '@angular/router';

@Component({
  selector: 'app-user-credits',
  templateUrl: './user-credits.component.html',
  styleUrls: ['./user-credits.component.scss']
})
export class UserCreditsComponent implements OnInit {

  userData: UserData;
  balanceForm = new FormGroup({
  });

  constructor(private userDataQuery: UserDataQuery, private buyCreditsCommand: BuyCreditsCommand, private router: Router) {
    this.fetchUserData();
  }

  private fetchUserData() {
    this.userDataQuery.execute().subscribe((v) => { this.userData = v; });
  }

  ngOnInit() {
  }

  onBalanceFormSubmit() {

  }

  onBuyCreditsClick() {
    this.buyCreditsCommand.execute().subscribe((v) => {
      if (v.status === 'COMPLETED') {
        this.userData.credits += 1000;
      } else {
        this.router.navigateByUrl('/error');
      }
    }, (err) => {
      console.log(err);
      this.router.navigateByUrl('/error');
    })
  }

}
