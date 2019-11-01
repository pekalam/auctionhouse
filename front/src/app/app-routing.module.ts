import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomePageComponent } from './home/pages/home-page/home-page.component';
import { CategoryPageComponent } from './categories/pages/category-page/category-page.component';
import { AuctionsPageComponent } from './auctions/pages/auctions-page/auctions-page.component';
import { SignInPageComponent } from './auth/pages/sign-in-page/sign-in-page.component';
import { SignUpPageComponent } from './auth/pages/sign-up-page/sign-up-page.component';
import { AuctionPageComponent } from './auctions/pages/auction-page/auction-page.component';
import { BidCreatePageComponent } from './bid/pages/bid-create-page/bid-create-page.component';
import { AuctionResolver } from './core/resolvers/auction.resolver';
import { AuctionCreatePageComponent } from './auctions/pages/auction-create-page/auction-create-page.component';


const routes: Routes = [
  { path: 'sign-up', component: SignUpPageComponent },
  { path: 'sign-in', component: SignInPageComponent },
  {
    path: 'createbid', component: BidCreatePageComponent, resolve:
      { auction: AuctionResolver }
  },
  { path: 'home', component: HomePageComponent },
  { path: 'category/:id', component: CategoryPageComponent },
  { path: '', pathMatch: 'full', redirectTo: 'home' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
