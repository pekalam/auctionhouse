import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Auction } from 'src/app/core/models/Auctions';
import { FormGroup, FormControl, Validators, ValidatorFn, AbstractControl, ValidationErrors } from '@angular/forms';
import { ProductFormValues } from 'src/app/shared/forms/product-step/product-step.component';
import { ProductFormResult } from 'src/app/shared/forms/product-step/productStep';
import { AuctionDataFormValues } from 'src/app/shared/forms/auction-data-step/auction-data-step.component';
import { AuctionDataStep } from '../../../shared/forms/auction-data-step/auctionDataStep';
import { AuctionImagesFormValues, AddImageFormResult } from '../../../shared/forms/add-image-step/add-image-step.component';
import { ImgSelectedEvent } from 'src/app/shared/forms/img-upload-input/img-upload-input.component';
import {DomSanitizer} from '@angular/platform-browser';
import { UserAddAuctionImageCommand } from '../../../core/commands/UserAddAuctionImageCommand';

@Component({
  selector: 'app-auction-edit-page',
  templateUrl: './auction-edit-page.component.html',
  styleUrls: ['./auction-edit-page.component.scss']
})
export class AuctionEditPageComponent implements OnInit {

  @ViewChild('categoryForm', { static: true })
  categoryForm;
  @ViewChild('productForm', { static: true })
  productForm;
  @ViewChild('imageStep', { static: true })
  imageStepComponent;
  @ViewChild('summaryStep', { static: true })
  summaryStepComponent;
  @ViewChild('auctionDataStep', { static: true })
  auctionDataStepComponent;

  auction: Auction;

  productFormDefaultValues: ProductFormValues;
  auctionDataFormValues: AuctionDataFormValues;
  auctionImagesFormValues: AuctionImagesFormValues;

  constructor(private router: Router, private sanitizer: DomSanitizer, private addImageCommand: UserAddAuctionImageCommand) {
    if (this.router.getCurrentNavigation().extras.state) {
      this.auction = this.router.getCurrentNavigation().extras.state.auction;
      this.createDefaultProductFormValues();
      this.createDefaultAuctionDataFormValues();
      this.createDefaultAuctionImagesFormValues();
    } else {
      this.router.navigate(['/user']);
    }
  }

  private createDefaultProductFormValues() {
    this.productFormDefaultValues = {
      productName: this.auction.product.name,
      condition: this.auction.product.condition,
      tags: this.auction.tags.join(' '),
      productDescription: this.auction.product.description
    }
  }

  private createDefaultAuctionDataFormValues() {
    this.auctionDataFormValues = {
      auction: !this.auction.buyNowOnly,
      buyNow: this.auction.buyNowOnly || this.auction.buyNowPrice != null,
      buyNowPrice: this.auction.buyNowPrice,
      name: this.auction.name,
      endDate: this.auction.endDate,
      startDate: this.auction.startDate
    }
  }

  private createDefaultAuctionImagesFormValues() {
    this.auctionImagesFormValues = {
      existingImages: this.auction.auctionImages
    }
    console.log(this.auctionImagesFormValues);

  }

  onProductFormSubmit(result: ProductFormResult) {
    console.log(result);

  }

  onAuctionDataFormSubmit(result: AuctionDataStep) {

  }

  onImageSelected(event: ImgSelectedEvent){
    const file = event.files.item(0);
    let url = URL.createObjectURL(file);
    let imgBlobUrl = this.sanitizer.bypassSecurityTrustUrl(url);
    this.imageStepComponent.setImgPreview(event.imgnum, imgBlobUrl);
  }

  onImagesFormSubmit(results: AddImageFormResult[]) {
    console.log("img  form result");
    console.log(results);
    for (const formResult of results) {
      if(formResult.added){
        this.addImageCommand.execute(formResult.files, formResult.num, this.auction.auctionId);
      }
    }
  }

  ngOnInit() {
  }

}
