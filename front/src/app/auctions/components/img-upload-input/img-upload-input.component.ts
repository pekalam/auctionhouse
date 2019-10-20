import { Component, OnInit, Input, HostListener, EventEmitter, Output, ViewChild, ElementRef } from '@angular/core';
import { HttpHeaders, HttpClient } from '@angular/common/http';
import { read } from 'fs';
import { AddAuctionImageCommand } from 'src/app/core/commands/AddAuctionImageCommand';
import { AuctionImageQuery } from 'src/app/core/queries/AuctionImageQuery';
import { RemoveAuctionImageCommand } from '../../../core/commands/RemoveAuctionImageCommand';

export class ImgUploadResult {
  constructor(public id: number, public file: string) { }
}

@Component({
  selector: 'app-img-upload-input',
  templateUrl: './img-upload-input.component.html',
  styleUrls: ['./img-upload-input.component.scss']
})
export class ImgUploadInputComponent implements OnInit {
  @Input('imgnum')
  imgNum: number;

  @Output()
  imgSelect = new EventEmitter<ImgUploadResult>();

  imgSrc = "";
  preview = true;

  @HostListener('change', ['$event.target.files']) emitFiles(event: FileList) {
    this.command.execute(event, this.imgNum).subscribe((msg) => {
      if (msg.result === 'completed') {
        console.log('add image completed');
        console.log(msg);
        this.fetchAuctionImagePreview(msg.values.imgSz1);
      } else {
        console.log('Cannot add image');
      }
    }, (err) => {
      console.log('Cannot add image');
    });
  }

  constructor(private command: AddAuctionImageCommand, private auctionImgQuery: AuctionImageQuery,
    private removeAuctionImageCommand: RemoveAuctionImageCommand) { }

  ngOnInit() {
  }

  private fetchAuctionImagePreview(imageId: string) {
    let imgUrl = this.auctionImgQuery.execute(imageId);
    this.imgSrc = imgUrl;
    this.preview = true;
  }


  onCancelClick() {
    this.removeAuctionImageCommand.execute(this.imgNum).subscribe((v) => {
      this.preview = false;
    }, (err) => {
      console.log("remove image error ");
      console.log(err);

    });
  }
}
