import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { ImgSelectedEvent } from '../img-upload-input/img-upload-input.component';
import { AuctionCreateStep } from '../auctionCreateStep';
import { AddAuctionImageCommand } from 'src/app/core/commands/AddAuctionImageCommand';
import { AuctionImageQuery } from 'src/app/core/queries/AuctionImageQuery';
import { RemoveAuctionImageCommand } from 'src/app/core/commands/RemoveAuctionImageCommand';
import { AuctionImage } from '../../../core/models/Auctions';

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
    this.show = new Array<number>(6).fill(null).map((v, ind) => values.existingImages[ind] ? 1 : 0);
    this.show[this.show.lastIndexOf(1) + 1 % this.show.length] = 1;
    this.previews = values.existingImages.map((v) => v ? this.auctionImgQuery.execute(v.size1Id) : null);
    for (let i = 0; i < this.previews.filter((v) => v != null).length; i++) {
      this.imgUploadResults[i] = { num: i, added: false, deleted: false, replaced: false, files: null };
    }
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

  constructor(private command: AddAuctionImageCommand,
    private auctionImgQuery: AuctionImageQuery,
    private removeAuctionImageCommand: RemoveAuctionImageCommand) {
    super();
    this.ready = true;
  }

  ngOnInit() {
  }

  setImgPreview(imgnum: number, imgSrc: string) {
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
    /*     this.removeAuctionImageCommand.execute(imgnum).subscribe((v) => {
          this.imgUploadResults[imgnum].imageId = null;
          this.previews[imgnum] = null;
        }, (err) => {
          console.log("remove image error ");
          console.log(err);
        }); */
  }

  onImgSelected(event: ImgSelectedEvent) {
    /*     this.command.execute(event.files, event.imgnum).subscribe((msg) => {
          if (msg.result === 'completed') {
            console.log('add image completed');
            console.log(msg);
            this.imgUploadResults[event.imgnum] = {
              imageId: msg.values.imgSz1,
              num: event.imgnum
            }
            this.previews[event.imgnum] = this.auctionImgQuery.execute(msg.values.imgSz1);
          } else {
            console.log('Cannot add image');
          }
        }, (err) => {
          console.log('Cannot add image');
        }); */
    this.imgUploadResults[event.imgnum] = {
      deleted: false,
      added: true,
      num: event.imgnum,
      files: event.files,
      replaced: false,
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
    const results = this.imgUploadResults.filter(i => i != null);
    if (this.defaultValues) {
      results.forEach((v) => {
        if (this.defaultValues.existingImages[v.num]) {
          v.replaced = v.added;
        }
      });
    }
    this.completeStep(results);
  }
}
