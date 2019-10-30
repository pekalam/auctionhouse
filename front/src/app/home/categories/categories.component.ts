import { Component, OnInit } from '@angular/core';
import { CategoryTreeNode } from 'src/app/core/models/CategoryTreeNode';
import { Router } from '@angular/router';
import { CategoriesQuery } from '../../core/queries/CategoriesQuery';
import { BreakpointObserver, BreakpointState } from '@angular/cdk/layout';


@Component({
  selector: 'home-categories',
  templateUrl: './categories.component.html',
  styleUrls: ['./categories.component.scss']
})
export class CategoriesComponent implements OnInit {

  mainCategories: CategoryTreeNode[];
  subCategories: CategoryTreeNode[];
  subcategoriesShown = false;
  selectedCategory: CategoryTreeNode;

  mobile = false;

  constructor(private categoriesQuery: CategoriesQuery, private router: Router, private breakpointObserver: BreakpointObserver) { }

  ngOnInit() {
    this.categoriesQuery
      .execute()
      .subscribe(categories => this.mainCategories = categories.subCategories);
    this.breakpointObserver
      .observe(['(max-width: 820px)'])
      .subscribe((state: BreakpointState) => {
        if (state.matches) {
          this.mobile = true;
        } else {
          this.mobile = false;
        }
      });
  }

  showSubcategories(category: CategoryTreeNode) {
    this.selectedCategory = category;
    this.subCategories = category.subCategories;
    this.subcategoriesShown = true;
  }

  hideSubcategories() {
    this.subcategoriesShown = false;
  }
}
