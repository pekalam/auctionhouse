import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CategoryTreeNode } from 'src/app/core/models/CategoryTreeNode';
import { CategoriesQuery } from 'src/app/core/queries/CategoriesQuery';

@Component({
  selector: 'app-category-page',
  templateUrl: './category-page.component.html',
  styleUrls: ['./category-page.component.scss']
})
export class CategoryPageComponent implements OnInit {
  category: CategoryTreeNode = {subCategories: [], categoryName: null};

  constructor(private activatedRoute: ActivatedRoute, private categoriesQuery: CategoriesQuery) {
    activatedRoute.params.subscribe(v => this.getCategoryTree(v.id));
  }

  getCategoryTree(categoryId: string) {
    this.categoriesQuery.getMainCategoryTree(categoryId).subscribe(v => this.category = v);
  }

  ngOnInit() {
  }

}
