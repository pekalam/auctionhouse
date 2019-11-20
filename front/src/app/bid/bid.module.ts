import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BidCreatePageComponent } from './pages/bid-create-page/bid-create-page.component';
import { AuctionPreviewComponent } from './components/auction-preview/auction-preview.component';
import { ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from '../material.module';
import { RouterModule } from '@angular/router';
import { BidRoutingModule } from './bid-routing.module';



@NgModule({
  declarations: [BidCreatePageComponent, AuctionPreviewComponent],
  imports: [
    ReactiveFormsModule,
    CommonModule,
    MaterialModule,
    RouterModule,
    BidRoutingModule
  ],
  exports: [BidCreatePageComponent]
})
export class BidModule { }
