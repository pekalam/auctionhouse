import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Auction } from '../models/Auctions';
import { Observable } from 'rxjs';
import { QueryHelper } from './QueryHelper';
import { environment } from '../../../environments/environment';


export interface UserAddress{
  street: string;
  city: string;
}

export interface UserData{
  username: string;
  address: UserAddress;
  credits: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserDataQuery {
  constructor(private httpClient: HttpClient, private queryHelper: QueryHelper) {

  }

  execute(): Observable<UserData>{
    const url = `${environment.API_URL}/api/userData`;
    let req = this.httpClient.get<UserData>(url);
    return this.queryHelper.pipeLoading(req);
  }
}
