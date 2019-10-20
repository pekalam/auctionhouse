import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BidCreatePageComponent } from './pages/bid-create-page/bid-create-page.component';
import { AuctionPreviewComponent } from './components/auction-preview/auction-preview.component';
import { ReactiveFormsModule } from '@angular/forms';



@NgModule({
  declarations: [BidCreatePageComponent, AuctionPreviewComponent],
  imports: [
    ReactiveFormsModule,
    CommonModule
  ],
  exports: [BidCreatePageComponent, AuctionPreviewComponent]
})
export class BidModule { }
