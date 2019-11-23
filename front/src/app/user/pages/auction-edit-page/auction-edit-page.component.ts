import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Auction } from 'src/app/core/models/Auctions';
import { FormGroup, FormControl, Validators, ValidatorFn, AbstractControl, ValidationErrors } from '@angular/forms';
import { ProductFormValues } from 'src/app/shared/forms/product-step/product-step.component';
import { ProductFormResult } from 'src/app/shared/forms/product-step/productStep';
import { AuctionDataFormValues } from 'src/app/shared/forms/auction-data-step/auction-data-step.component';
import { AuctionDataStep } from '../../../shared/forms/auction-data-step/auctionDataStep';
import { AuctionImagesFormValues, AddImageFormResult } from '../../../shared/forms/add-image-step/add-image-step.component';
import { ImgSelectedEvent } from 'src/app/shared/forms/img-upload-input/img-upload-input.component';
import { DomSanitizer } from '@angular/platform-browser';
import { UserAddAuctionImageCommand } from '../../../core/commands/UserAddAuctionImageCommand';
import { UpdateAuctionCommand, UpdateAuctionCommandArgs } from '../../../core/commands/UpdateAuctionCommand';
import { UserRemoveAuctionImageCommand } from '../../../core/commands/UserRemoveAuctionImageCommand';
import { UserReplaceAuctionImageCommand } from '../../../core/commands/UserReplaceAuctionImageCommand';
import { ServerMessageService } from '../../../core/services/ServerMessageService';
import { first } from 'rxjs/operators';
import { Subscription } from 'rxjs';
import { CategorySelectStep } from '../../../shared/forms/category-select-step/categorySelectStep';

@Component({
  selector: 'app-auction-edit-page',
  templateUrl: './auction-edit-page.component.html',
  styleUrls: ['./auction-edit-page.component.scss']
})
export class AuctionEditPageComponent implements OnInit, OnDestroy {
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

  private subscription: Subscription;

  constructor(private router: Router, private sanitizer: DomSanitizer,
              private userAddImageCommand: UserAddAuctionImageCommand,
              private userRemoveAuctionImageCommand: UserRemoveAuctionImageCommand,
              private userReplaceAuctionImageCommand: UserReplaceAuctionImageCommand,
              private updateAuctionCommand: UpdateAuctionCommand,
              private serverMessageService: ServerMessageService) {
    if (this.router.getCurrentNavigation().extras.state) {
      this.auction = this.router.getCurrentNavigation().extras.state.auction;
      this.createDefaultProductFormValues();
      this.createDefaultAuctionDataFormValues();
      this.createDefaultAuctionImagesFormValues();
    } else {
      this.router.navigate(['/user']);
    }
    this.subscription = this.serverMessageService.connectionStarted.subscribe((v) => !v ? this.router.navigateByUrl('/error') : null);
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
  }

  private createUpdateCommandArgsFromAuction(): UpdateAuctionCommandArgs {
    return {
      auctionId: this.auction.auctionId,
      buyNowPrice: this.auction.buyNowPrice,
      category: [this.auction.category.name, this.auction.category.subCategory.name, this.auction.category.subCategory.subCategory.name],
      description: this.auction.product.description,
      endDate: this.auction.endDate,
      tags: this.auction.tags,
      name: this.auction.name
    };
  }

  private updateAuction(args: UpdateAuctionCommandArgs) {
    this.updateAuctionCommand.execute(args).pipe(first()).subscribe((v) => console.log("Auction updated"), (err) => {
      console.log(err);
      this.router.navigateByUrl('/error');
    });
  }

  private addImage(formResult: AddImageFormResult) {
    this.userAddImageCommand
      .execute(formResult.files, formResult.num, this.auction.auctionId)
      .subscribe((msg) => console.log("image added"), (err) => this.router.navigateByUrl('/error'));
  }

  private replaceImage(formResult: AddImageFormResult) {
    this.userReplaceAuctionImageCommand
      .execute(formResult.files, formResult.num, this.auction.auctionId)
      .subscribe((msg) => console.log("image replaced"), (err) => this.router.navigateByUrl('/error'));
  }

  private removeImage(num: number){
    this.userRemoveAuctionImageCommand
      .execute(num, this.auction.auctionId)
      .subscribe((msg) => console.log(`image ${num} removed`), (err) => this.router.navigateByUrl('/error'));
  }


  onProductFormSubmit(result: ProductFormResult) {
    let commandArgs = this.createUpdateCommandArgsFromAuction();
    commandArgs.description = result.product.description;
    commandArgs.tags = result.tags;
    this.updateAuction(commandArgs);
  }

  onAuctionDataFormSubmit(result: AuctionDataStep) {
    console.log(result);

    let commandArgs = this.createUpdateCommandArgsFromAuction();
    commandArgs.buyNowPrice = result.buyNow ? result.buyNowPrice : null;
    commandArgs.endDate = result.endDate === this.auction.endDate ? null : result.endDate;
    commandArgs.name = result.name;
    this.updateAuction(commandArgs);
  }

  onCategoriesFormSubmit(result: CategorySelectStep) {
    let commandArgs = this.createUpdateCommandArgsFromAuction();
    commandArgs.category = [
      result.selectedMainCategory.categoryName,
      result.selectedSubCategory.categoryName,
      result.selectedSubCategory2.categoryName
    ];
    this.updateAuction(commandArgs);
  }

  onImageSelected(event: ImgSelectedEvent) {
    const file = event.files.item(0);
    let url = URL.createObjectURL(file);
    let imgBlobUrl = this.sanitizer.bypassSecurityTrustUrl(url);
    this.imageStepComponent.setImgPreview(event.imgnum, imgBlobUrl);
  }

  onImagesFormSubmit(results: AddImageFormResult[]) {
    console.log('img  form result');
    console.log(results);
    for (const formResult of results) {
      if (formResult.added) {
        this.addImage(formResult);
      } else if (formResult.deleted) {
        this.removeImage(formResult.num);
      } else if (formResult.replaced) {
        this.replaceImage(formResult);
      }
    }
  }

  ngOnInit() {
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

}
