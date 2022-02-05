import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomePageComponent } from './pages/home-page/home-page.component';
import { CategoriesComponent } from './components/categories/categories.component';
import { RouterModule } from '@angular/router';
import { LayoutModule } from '@angular/cdk/layout';
import { SharedModule } from '../shared/shared.module';
import { AuctionsCarouselComponent } from './components/auctions-carousel/auctions-carousel.component';
import { HomeRoutingModule } from './home-routing.module';
import { RecentlyViewedComponent } from './components/recently-viewed/recently-viewed.component';
import { EndingAuctionsComponent } from './components/ending-auctions/ending-auctions.component';
import { MaterialModule } from '../material.module';
import { ReactiveFormsModule } from '@angular/forms';



@NgModule({
  declarations: [HomePageComponent, CategoriesComponent, AuctionsCarouselComponent, RecentlyViewedComponent, EndingAuctionsComponent],
  imports: [
    CommonModule,
    RouterModule,
    LayoutModule,
    SharedModule,
    HomeRoutingModule,
    MaterialModule,
    ReactiveFormsModule,
  ]
})
export class HomeModule { }
