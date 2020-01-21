import { Injectable } from '@angular/core';
import { UserStorageService } from './UserStorageService';
import { Auction } from '../models/Auctions';
import { AuthenticationStateService } from './AuthenticationStateService';
import { Observable } from 'rxjs';
import { map, first } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class RecentlyViewedService {

  static MAX_VIEWED = 3;

  constructor(private userStorageService: UserStorageService, private authenticationStateService: AuthenticationStateService) { }

  getRecentlyViewedAuctions(): Observable<Auction[]> {
    let ret =  this.authenticationStateService.currentUser.pipe(
      first(),
      map((user, ind) => {
        const data = this.userStorageService.getData(user.userId);
        if (data) {
          return data.recentlyViewed;
        } else {
          return [];
        }
      })
    );
    this.authenticationStateService.checkIsAuthenticated();
    return ret;
  }

  private _addRecentlyViewed(userId: string, auction: Auction) {
    let data = this.userStorageService.getData(userId);
    if (data === null) {
      data = {
        recentlyViewed: []
      };
    }
    let auctions = data.recentlyViewed as Auction[];
    if (auctions.find((a) => a.auctionId === auction.auctionId)) {
      return;
    }
    if (auctions.length === RecentlyViewedService.MAX_VIEWED) {
      auctions = auctions.slice(1);
    }
    auctions.push(auction);
    data.recentlyViewed = auctions;
    this.userStorageService.setData(userId, data);
  }

  addRecentlyViewed(auction: Auction) {
    this.authenticationStateService.currentUser.pipe(
      first()
    ).subscribe((user) => {
      this._addRecentlyViewed(user.userId, auction);
    });
    this.authenticationStateService.checkIsAuthenticated();
  }
}
