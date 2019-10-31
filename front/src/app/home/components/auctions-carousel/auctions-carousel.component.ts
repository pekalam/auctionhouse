import { Component, OnInit } from '@angular/core';
import { AuctionListModel } from '../../../core/models/Auctions';

@Component({
  selector: 'app-auctions-carousel',
  templateUrl: './auctions-carousel.component.html',
  styleUrls: ['./auctions-carousel.component.scss']
})
export class AuctionsCarouselComponent implements OnInit {

  auctions: any[] = [
    {productName: "Item 1", price: 20.20},
    {productName: "Item 2", price: 99.99}
  ]
  selectedImg = 0;

  constructor() { }

  ngOnInit() {
  }

  f(n){
    console.log(n);

  }
}
