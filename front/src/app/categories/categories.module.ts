import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CategoryPageComponent } from './pages/category-page/category-page.component';
import { Routes, RouterModule } from '@angular/router';


@NgModule({
  declarations: [CategoryPageComponent],
  imports: [
    CommonModule,
    RouterModule
  ]
})
export class CategoriesModule { }
