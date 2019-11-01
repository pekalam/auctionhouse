import { Component, OnInit } from '@angular/core';
import { Subject, Observable, of } from 'rxjs';
import { TopAuctionsQueryResult, TopAuctionQueryItem, TopAuctionsByTagQuery } from 'src/app/core/queries/TopAuctionsByTagQuery';
import { switchMap, distinctUntilChanged, debounceTime, catchError } from 'rxjs/operators';

@Component({
  selector: 'app-search-bar',
  templateUrl: './search-bar.component.html',
  styleUrls: ['./search-bar.component.scss']
})
export class SearchBarComponent implements OnInit {

  private searchVal = new Subject<string>();
  autocompleteResults: Observable<TopAuctionsQueryResult>;
  auctions: TopAuctionQueryItem[] = [];
  total = 0;
  tag;

  constructor(private tagsQuery: TopAuctionsByTagQuery) {


    this.autocompleteResults = this.searchVal.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      switchMap((s) => {

          return this.tagsQuery.execute(s, 0).pipe(catchError(err => of(null)))
      })
    );

    this.autocompleteResults.subscribe((v) => {
      if (!v) {
        this.auctions = [];
        return;
      }
      this.auctions = v.auctions;
      this.total = v.total;
      this.tag = v.tag;
    });
  }

  onSearchBarKey(val) {
    console.log("sear");
    if(val.length > 0){
      this.searchVal.next(val);
    }
  }

  ngOnInit() {
  }

}
