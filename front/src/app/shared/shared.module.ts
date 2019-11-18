import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationComponent } from './navigation/navigation.component';
import { RouterModule } from '@angular/router';
import { CoreModule } from '../core/core.module';
import { MaterialModule } from '../material.module';
import { VisibleDirective } from './visible.directive';
import { SearchBarComponent } from './search-bar/search-bar.component';
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
    NavigationComponent,
    VisibleDirective,
    SearchBarComponent,
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
    NavigationComponent,
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
