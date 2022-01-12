import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { UserPageComponent } from './pages/user-page/user-page.component';
import { AuthGuard } from '../core/auth/AuthGuard';
import { AuctionEditPageComponent } from './pages/auction-edit-page/auction-edit-page.component';
import { ServerConnectionGuard } from '../core/guards/ServerConnectionGuard';

const routes: Routes = [
  { path: 'user', pathMatch: 'full', component: UserPageComponent, canActivate: [AuthGuard, /*ServerConnectionGuard*/] },
  { path: 'editAuction', pathMatch: 'full', component: AuctionEditPageComponent, canActivate: [AuthGuard, /*ServerConnectionGuard*/] },

];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UserRoutingModule { }
