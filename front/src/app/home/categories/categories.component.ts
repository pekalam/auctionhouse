import { Component, OnInit } from '@angular/core';
import { CategoryTreeNode } from 'src/app/core/models/CategoryTreeNode';
import { Router } from '@angular/router';
import { CategoriesQuery } from '../../core/queries/CategoriesQuery';

@Component({
  selector: 'home-categories',
  templateUrl: './categories.component.html',
  styleUrls: ['./categories.component.scss']
})
export class CategoriesComponent implements OnInit {

  mainCategories: CategoryTreeNode[];
  subCategories: CategoryTreeNode[];
  subcategoriesShown = false;

  constructor(private categoriesQuery: CategoriesQuery, private router: Router) { }

  ngOnInit() {
    this.categoriesQuery
      .execute()
      .subscribe(categories => this.mainCategories = categories.subCategories);
  }

  showSubcategories(category: CategoryTreeNode) {
    this.subCategories = category.subCategories;
    this.subcategoriesShown = true;
  }

  hideSubcategories() {
    this.subcategoriesShown = false;
  }
}
