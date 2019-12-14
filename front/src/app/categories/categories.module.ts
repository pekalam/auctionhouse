import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CategoryPageComponent } from './pages/category-page/category-page.component';
import { Routes, RouterModule } from '@angular/router';
import { CategoriesRoutingModule } from './categories-routing.module';


@NgModule({
  declarations: [CategoryPageComponent],
  imports: [
    CommonModule,
    RouterModule,
    CategoriesRoutingModule
  ]
})
export class CategoriesModule { }
