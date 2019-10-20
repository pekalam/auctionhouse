import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Auction } from '../models/Auctions';
import { Observable } from 'rxjs';

export interface UserAddress{
  street: string;
  city: string;
}

export interface UserData{
  userName: string;
  address: UserAddress;
}

@Injectable({
  providedIn: 'root'
})
export class UserDataQuery {
  constructor(private httpClient: HttpClient) {

  }

  execute(): Observable<UserData>{
    const url = '/api/userData';
    return this.httpClient.get<UserData>(url);
  }
}
