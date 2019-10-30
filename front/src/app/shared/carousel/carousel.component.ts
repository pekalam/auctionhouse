import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-carousel',
  templateUrl: './carousel.component.html',
  styleUrls: ['./carousel.component.scss']
})
export class CarouselComponent implements OnInit {

  @Input('img-height')
  imgHeight = 250;
  imgs = [];
  imgInd = [];
  shown = 0;

  @Input('sources')
  set sources(srcs: Array<string>) {
    console.log(srcs);

    this.imgs = srcs;
    this.imgInd = this.imgs.map((v, i) => i);
  }



  constructor() { }

  ngOnInit() {
  }

}
