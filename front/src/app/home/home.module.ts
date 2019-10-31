import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomePageComponent } from './pages/home-page/home-page.component';
import { CategoriesComponent } from './categories/categories.component';
import { RouterModule } from '@angular/router';
import { LayoutModule } from '@angular/cdk/layout';
import { SharedModule } from '../shared/shared.module';
import { AuctionsCarouselComponent } from './components/auctions-carousel/auctions-carousel.component';



@NgModule({
  declarations: [HomePageComponent, CategoriesComponent, AuctionsCarouselComponent],
  imports: [
    CommonModule,
    RouterModule,
    LayoutModule,
    SharedModule
  ]
})
export class HomeModule { }
