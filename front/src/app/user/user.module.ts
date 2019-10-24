import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserPageComponent } from './pages/user-page/user-page.component';
import { MaterialModule } from '../material.module';
import { UserRoutingModule } from './user-routing.module';
import { RouterModule } from '@angular/router';
import { UserDataPageComponent } from './pages/user-page/subpages/user-data-page/user-data-page.component';
import { UserAuctionsPageComponent } from './pages/user-page/subpages/user-auctions-page/user-auctions-page.component';
import { UserAuctionsListItemComponent } from './components/user-auctions-list-item/user-auctions-list-item.component';



@NgModule({
  declarations: [UserPageComponent, UserDataPageComponent, UserAuctionsPageComponent, UserAuctionsListItemComponent],
  imports: [
    CommonModule,
    MaterialModule,
    UserRoutingModule,
    RouterModule
  ]
})
export class UserModule { }
