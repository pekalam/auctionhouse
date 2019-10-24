import { Component, OnInit } from '@angular/core';
import { ImgUploadResult } from '../../../../components/img-upload-input/img-upload-input.component';
import { AuctionCreateStep } from '../../../../auctionCreateStep';

@Component({
  selector: 'app-add-image-step',
  templateUrl: './add-image-step.component.html',
  styleUrls: ['./add-image-step.component.scss']
})
export class AddImageStepComponent extends AuctionCreateStep<ImgUploadResult[]> implements OnInit {

  imgIds = [0, 1, 2, 3, 4, 5];
  show = [1, 1, 1, 0, 0, 0];

  imgUploadResults: Array<ImgUploadResult> = new Array<ImgUploadResult>(6);

  constructor() { super(); }

  ngOnInit() {
  }

  onAddImg(result: ImgUploadResult) {
    this.imgUploadResults[result.id] = result;
  }

  onImgSelect(result: ImgUploadResult) {
    let found = this.imgIds.find(v => v == result.id);
    if (found) {
      if (found !== this.show.length - 1) {
        this.show[found + 1] = 1;
      }
    }
  }

  onOK() {
    let results = this.imgUploadResults.filter(i => i != null);
    this.completeStep(results);
  }
}
