import { Component, OnInit, Input, HostListener, EventEmitter, Output, ViewChild, ElementRef } from '@angular/core';


export interface ImgSelectedEvent{
  files: FileList;
  imgnum: number;
}

@Component({
  selector: 'app-img-upload-input',
  templateUrl: './img-upload-input.component.html',
  styleUrls: ['./img-upload-input.component.scss']
})
export class ImgUploadInputComponent implements OnInit {

  previewsrc: string;

  @Input()
  imgnum: number;

  @Input('previewsrc')
  set setPreviewSrc(src: string){
    this.previewsrc = src;
    this.loading = true;
  }

  @Output()
  cancel = new EventEmitter<number>();

  @Output()
  imgSelected = new EventEmitter<ImgSelectedEvent>();

  @HostListener('change', ['$event.target.files']) emitFiles(event: FileList) {
    this.imgSelected.emit({files: event, imgnum: this.imgnum});
  }

  loading = false;

  onImgLoaded(){
    this.loading = false;
  }

  constructor() { }

  ngOnInit() {
  }
}
