import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CoreModule } from '../core/core.module';
import { MaterialModule } from '../material.module';
import { VisibleDirective } from './visible.directive';
import { CarouselComponent } from './carousel/carousel.component';
import { ProductStepComponent } from './forms/product-step/product-step.component';
import { ReactiveFormsModule } from '@angular/forms';
import { CategorySelectStepComponent } from './forms/category-select-step/category-select-step.component';
import { AuctionDataStepComponent } from './forms/auction-data-step/auction-data-step.component';
import { AddImageStepComponent } from './forms/add-image-step/add-image-step.component';
import { ImgUploadInputComponent } from './forms/img-upload-input/img-upload-input.component';
import { CreateSummaryStepComponent } from './forms/create-summary-step/create-summary-step.component';




@NgModule({
  declarations: [
    VisibleDirective,
    CarouselComponent,
    ProductStepComponent,
    CategorySelectStepComponent,
    AuctionDataStepComponent,
    AddImageStepComponent,
    ImgUploadInputComponent,
    CreateSummaryStepComponent
  ],
  imports: [
    CoreModule,
    CommonModule,
    MaterialModule,
    RouterModule,
    ReactiveFormsModule
  ],
  exports: [
    VisibleDirective,
    CarouselComponent,
    ProductStepComponent,
    CategorySelectStepComponent,
    AuctionDataStepComponent,
    AddImageStepComponent,
    ImgUploadInputComponent,
    CreateSummaryStepComponent
  ]
})
export class SharedModule { }
