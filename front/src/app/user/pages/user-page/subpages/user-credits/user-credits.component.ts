import { Component, OnInit } from '@angular/core';
import { UserDataQuery, UserData } from '../../../../../core/queries/UserDataQuery';
import { FormGroup, FormControl } from '@angular/forms';

@Component({
  selector: 'app-user-credits',
  templateUrl: './user-credits.component.html',
  styleUrls: ['./user-credits.component.scss']
})
export class UserCreditsComponent implements OnInit {

  userData: UserData;
  balanceForm = new FormGroup({
  });

  constructor(private userDataQuery: UserDataQuery) {
    userDataQuery.execute().subscribe((v) => { this.userData = v; });
  }

  ngOnInit() {
  }

  onBalanceFormSubmit(){

  }

}
