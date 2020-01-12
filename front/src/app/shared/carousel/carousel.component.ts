import { Component, OnInit, Input, Output, EventEmitter, ViewChild, ElementRef, AfterViewInit } from '@angular/core';

@Component({
  selector: 'app-carousel',
  templateUrl: './carousel.component.html',
  styleUrls: ['./carousel.component.scss']
})
export class CarouselComponent implements OnInit, AfterViewInit {

  @ViewChild('image', { static: false })
  image: ElementRef;

  @Input('img-height')
  imgHeight = 250;
  imgs = [];
  imgInd = [];
  shown = 0;
  loading = true;

  @Input()
  imageButtons = true;

  @Input('sources')
  set sources(srcs: Array<string>) {
    console.log(srcs);

    this.imgs = srcs;
    this.imgInd = this.imgs.map((v, i) => i);
    this.loading = true;
  }

  @Output('imgSelected')
  imgSelected = new EventEmitter<number>();


  constructor() { }

  ngOnInit() {

  }

  ngAfterViewInit() {

  }

  onImgLoaded(imgInd: number){
    this.loading = imgInd == this.shown ? false : this.loading;
  }

  onPrev() {
    this.shown = this.shown - 1 < 0 ? this.imgInd.length - 1 : this.shown - 1;
    this.imgSelected.emit(this.shown);
  }

  onNext() {
    this.shown = (this.shown + 1) % this.imgInd.length;
    this.imgSelected.emit(this.shown);
  }
}
