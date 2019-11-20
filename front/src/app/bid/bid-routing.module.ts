import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../core/auth/AuthGuard';
import { ServerConnectionGuard } from '../core/guards/ServerConnectionGuard';
import { BidCreatePageComponent } from './pages/bid-create-page/bid-create-page.component';
import { AuctionResolver } from '../core/resolvers/auction.resolver';

const routes: Routes = [
  {
    path: 'createbid', component: BidCreatePageComponent,
    resolve: { auction: AuctionResolver }, canActivate: [AuthGuard, ServerConnectionGuard]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class BidRoutingModule { }
