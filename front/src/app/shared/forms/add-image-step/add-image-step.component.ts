import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { ImgSelectedEvent } from '../img-upload-input/img-upload-input.component';
import { AuctionCreateStep } from '../auctionCreateStep';
import { AddAuctionImageCommand } from 'src/app/core/commands/auction/AddAuctionImageCommand';
import { AuctionImageQuery } from 'src/app/core/queries/AuctionImageQuery';
import { RemoveAuctionImageCommand } from 'src/app/core/commands/auction/RemoveAuctionImageCommand';
import { AuctionImage } from '../../../core/models/Auctions';
import { ValidatorFn } from '@angular/forms';

export interface AddImageFormResult {
  num: number;
  added: boolean;
  deleted: boolean;
  replaced: boolean;
  files: FileList;
}

export interface AuctionImagesFormValues {
  existingImages: AuctionImage[];
}

@Component({
  selector: 'app-add-image-step',
  templateUrl: './add-image-step.component.html',
  styleUrls: ['./add-image-step.component.scss']
})
export class AddImageStepComponent extends AuctionCreateStep<AddImageFormResult[]> implements OnInit {


  @Input('defaults')
  set setDefaults(values: AuctionImagesFormValues) {
    if(!values){return;}
    this.show = new Array<number>(6).fill(null).map((_, ind) => (values.existingImages[ind] || ind < 3) ? 1 : 0);
    this.show[this.show.lastIndexOf(1) + 1 % this.show.length] = 1;
    this.previews = new Array<number>(6)
      .fill(null)
      .map((v, ind) => values.existingImages[ind] ? this.auctionImgQuery.execute(values.existingImages[ind].size1Id) : null);
    this.defaultValues = values;
  }

  @Output()
  imgCanceled = new EventEmitter<number>();

  @Output()
  imgSelected = new EventEmitter<ImgSelectedEvent>();


  private defaultValues: AuctionImagesFormValues;
  imgIds = new Array<number>(6).fill(null).map((_, ind) => ind);
  show = new Array<number>(6).fill(null).map((_, ind) => ind < 3 ? 1 : 0);
  previews = new Array<string>(6).fill(null);
  imgUploadResults: Array<AddImageFormResult> = new Array<AddImageFormResult>(6);
  titleMsg = 'Add images';

  constructor(private auctionImgQuery: AuctionImageQuery) {
    super();
    this.ready = true;
    this.resetResults();
  }

  private resetResults(){
    for (let i = 0; i < this.imgUploadResults.length; i++) {
      this.imgUploadResults[i] = { num: i, added: false, deleted: false, replaced: false, files: null };
    }
  }

  ngOnInit() {
  }

  setImgPreview(imgnum: number, imgSrc: string) {
    console.log("set preview" +imgnum+ "to " + imgSrc);

    this.previews[imgnum] = imgSrc;
  }

  onImgCancel(imgnum: number) {
    this.imgCanceled.emit(imgnum);
    this.previews[imgnum] = null;
    this.imgUploadResults[imgnum] = {
      deleted: true,
      added: false,
      num: imgnum,
      files: null,
      replaced: false,
    };
  }

  onImgSelected(event: ImgSelectedEvent) {
    this.imgUploadResults[event.imgnum] = {
      deleted: false,
      added: this.defaultValues ? this.defaultValues.existingImages[event.imgnum] == null : true,
      num: event.imgnum,
      files: event.files,
      replaced: this.defaultValues ? this.defaultValues.existingImages[event.imgnum] != null : false,
    };
    this.imgSelected.emit(event);
    const found = this.imgIds.find(v => v === event.imgnum);
    if (found) {
      if (found !== this.show.length - 1) {
        this.show[found + 1] = 1;
      }
    }
  }

  onOkClick() {
    this.completeStep(this.imgUploadResults);
    this.resetResults();
  }
}
