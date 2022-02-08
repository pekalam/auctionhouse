import { Component, OnInit, ViewChild } from '@angular/core';
import { Subject, Observable, of, EMPTY } from 'rxjs';
import { TopAuctionsQueryResult, TopAuctionQueryItem, TopAuctionsByTagQuery } from 'src/app/core/queries/TopAuctionsByTagQuery';
import { switchMap, distinctUntilChanged, debounceTime, catchError } from 'rxjs/operators';
import { TopAuctionsByProductNameQuery, TopAuctionsByProductNameQueryResult } from '../../core/queries/TopAuctionsByProductNameQuery';
import { MatAutocompleteTrigger } from '@angular/material';
import { AuctionImageQuery } from '../../core/queries/AuctionImageQuery';

@Component({
  selector: 'app-search-bar',
  templateUrl: './search-bar.component.html',
  styleUrls: ['./search-bar.component.scss']
})
export class SearchBarComponent implements OnInit {

  @ViewChild(MatAutocompleteTrigger, { static: true })
  searchBar: MatAutocompleteTrigger;

  private searchVal = new Subject<string>();
  private byTagResults: Observable<TopAuctionsQueryResult>;
  private byProductNameResults: Observable<TopAuctionsByProductNameQueryResult[]>;


  auctionsByTag: TopAuctionQueryItem[] = [];
  totalInTag = 0;
  tag;
  byTagLoading = false;

  auctionsByProductName: TopAuctionQueryItem[] = [];
  totalByProductName = 0;
  byProcuctLoading = false;

  constructor(private tagsQuery: TopAuctionsByTagQuery, private topAuctionsByProductNameQuery: TopAuctionsByProductNameQuery,
              private auctionImageQuery: AuctionImageQuery) {

    this.byTagResults = this.searchVal.pipe(
      debounceTime(500),
      switchMap((s) => {
        this.byTagLoading = true;
        return this.tagsQuery.execute(s, 0);
      })
    );

    this.byTagResults.subscribe((v) => {
      this.byTagLoading = false;
      if (!v) {
        this.auctionsByTag = [];
        return;
      }
      this.auctionsByTag = v.auctions;
      this.totalInTag = v.total;
      this.tag = v.tag;
    }, (err) => { this.byTagLoading = false; });


    this.byProductNameResults = this.searchVal.pipe(
      debounceTime(500),
      switchMap((s) => {
        this.byProcuctLoading = true;
        return this.topAuctionsByProductNameQuery.execute(s, 0).pipe(catchError((err) => EMPTY));
      })
    );

    this.byProductNameResults.subscribe((v) => {
      this.byProcuctLoading = false;
      if (!v) {
        this.auctionsByProductName = [];
        return;
      }
      this.auctionsByProductName = v.map(v => v.auctions).reduce((prev, curr) => prev.concat(curr), []);
      this.totalByProductName = v.reduce((prev, curr) => prev + curr.total, 0);
    }, (err) => { this.byProcuctLoading = false; });
  }

  openAutocomplete(event) {
    console.log(this.searchBar);
    event.stopPropagation();
    this.searchBar.openPanel();
  }

  onSearchBarKey(val) {
    if (val.length > 0) {
      this.searchVal.next(val);
    }
  }

  getImageUrl(imageId: string): string{
    return this.auctionImageQuery.execute(imageId);
  }

  ngOnInit() {
  }

}
