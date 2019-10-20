import { Component, OnInit } from '@angular/core';
import { UserDataQuery, UserData } from '../../../../../core/queries/UserDataQuery';

@Component({
  selector: 'app-user-data-page',
  templateUrl: './user-data-page.component.html',
  styleUrls: ['./user-data-page.component.scss']
})
export class UserDataPageComponent implements OnInit {

  userData: UserData;
  error: string;

  constructor(userDataQuery: UserDataQuery) {
    userDataQuery.execute().subscribe((data) => {
      this.displayFetchedData(data);
    })
  }

  displayFetchedData(data){
    if (data) {
      this.userData = data;
    }else{
      this.error = "Cannot fetch user data"
    }
  }

  ngOnInit() {
  }

}
