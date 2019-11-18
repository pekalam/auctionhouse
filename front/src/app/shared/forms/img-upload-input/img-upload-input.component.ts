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

  @Input()
  imgnum: number;

  @Input()
  previewsrc: string;

  @Output()
  cancel = new EventEmitter<number>();

  @Output()
  imgSelected = new EventEmitter<ImgSelectedEvent>();

  @HostListener('change', ['$event.target.files']) emitFiles(event: FileList) {
    this.imgSelected.emit({files: event, imgnum: this.imgnum});
  }

  constructor() { }

  ngOnInit() {
  }
}
