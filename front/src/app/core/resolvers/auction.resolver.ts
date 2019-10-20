import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { AuctionQuery } from '../queries/AuctionQuery';
import { Auction } from '../models/Auctions';

@Injectable({ providedIn: 'root' })
export class AuctionResolver implements Resolve<Auction> {
  constructor(private auctionQuery: AuctionQuery) {

  }

  resolve(route: ActivatedRouteSnapshot): Observable<Auction> {
    let auctionId = route.queryParams.auctionId;
    return this.auctionQuery.execute(auctionId);
  }
}
