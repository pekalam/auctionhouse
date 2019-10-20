import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuctionsPageComponent } from './pages/auctions-page/auctions-page.component';
import { AuctionComponent } from './components/auction/auction.component';
import { AuctionListItemComponent } from './components/auction-list-item/auction-list-item.component';
import { AuctionPageComponent } from './pages/auction-page/auction-page.component';
import { RouterModule } from '@angular/router';
import { AuctionCreatePageComponent } from './pages/auction-create-page/auction-create-page.component';
import { ReactiveFormsModule } from '@angular/forms';
import { AuctionsRoutingModule } from './auctions-routing.module';
import { MaterialModule } from '../material.module';
import { CategorySelectStepComponent } from './pages/auction-create-page/steps/category-select-step/category-select-step.component';
import { ProductStepComponent } from './pages/auction-create-page/steps/product-step/product-step.component';
import { SharedModule } from '../shared/shared.module';
import { AddImageStepComponent } from './pages/auction-create-page/steps/add-image-step/add-image-step.component';
import { ImgUploadInputComponent } from './components/img-upload-input/img-upload-input.component';
import { CreateSummaryStepComponent } from './pages/auction-create-page/steps/create-summary-step/create-summary-step.component';
import { AuctionFiltersComponent } from './components/auction-filters/auction-filters.component';



@NgModule({
  declarations: [AuctionsPageComponent, AuctionComponent, AuctionListItemComponent, AuctionPageComponent, AuctionCreatePageComponent,
    CategorySelectStepComponent,
    ProductStepComponent,
    AddImageStepComponent,
    ImgUploadInputComponent,
    CreateSummaryStepComponent,
    AuctionFiltersComponent],
  imports: [
    AuctionsRoutingModule,
    ReactiveFormsModule,
    MaterialModule,
    RouterModule,
    CommonModule,
    SharedModule
  ]
})
export class AuctionsModule { }
