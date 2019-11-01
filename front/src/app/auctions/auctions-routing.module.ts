import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuctionsPageComponent } from './pages/auctions-page/auctions-page.component';
import { AuctionCreatePageComponent } from './pages/auction-create-page/auction-create-page.component';
import { AuthGuard } from '../core/auth/AuthGuard';
import { AuctionPageComponent } from './pages/auction-page/auction-page.component';

const routes: Routes = [
  { path: 'auction', component: AuctionPageComponent },
  { path: 'auctions/:mainCategory/:subCategory/:subCategory2', component: AuctionsPageComponent },
  { path: 'auctions/create', canActivate: [AuthGuard],
    data: { msg: 'You must be signed in to create an auction' }, component: AuctionCreatePageComponent },
  { path: 'auctions', component: AuctionsPageComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuctionsRoutingModule { }
