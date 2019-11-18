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
import { SharedModule } from '../shared/shared.module';
import { AuctionFiltersComponent } from './components/auction-filters/auction-filters.component';
import { LayoutModule } from '@angular/cdk/layout';
import { ErrorModule } from '../error/error.module';


@NgModule({
  declarations: [AuctionsPageComponent,
    AuctionComponent,
    AuctionListItemComponent,
    AuctionPageComponent,
    AuctionCreatePageComponent,
    AuctionFiltersComponent,
  ],
  imports: [
    AuctionsRoutingModule,
    ReactiveFormsModule,
    MaterialModule,
    RouterModule,
    CommonModule,
    SharedModule,
    LayoutModule,
    ErrorModule
  ]
})
export class AuctionsModule { }
