import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomePageComponent } from './pages/home-page/home-page.component';
import { CategoriesComponent } from './categories/categories.component';
import { RouterModule } from '@angular/router';



@NgModule({
  declarations: [HomePageComponent, CategoriesComponent],
  imports: [
    CommonModule,
    RouterModule
  ]
})
export class HomeModule { }
