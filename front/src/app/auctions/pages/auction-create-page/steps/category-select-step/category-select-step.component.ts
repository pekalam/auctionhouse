import { Component, OnInit, Input } from '@angular/core';
import { CategoryTreeNode } from 'src/app/core/models/CategoryTreeNode';
import { CategoriesQuery } from 'src/app/core/queries/CategoriesQuery';
import { CategorySelectStep } from '../../../../categorySelectStep';
import { AuctionCreateStep } from '../../../../auctionCreateStep';

@Component({
  selector: 'app-category-select-step',
  templateUrl: './category-select-step.component.html',
  styleUrls: ['./category-select-step.component.scss']
})
export class CategorySelectStepComponent extends AuctionCreateStep<CategorySelectStep> implements OnInit {
  mainCategories: CategoryTreeNode[] = [];
  subCategories: CategoryTreeNode[] = [];
  subCategories2: CategoryTreeNode[] = [];
  selectedMainCategory: CategoryTreeNode;
  selectedSubCategory: CategoryTreeNode;
  selectedSubCategory2: CategoryTreeNode;


  constructor(private categoriesQuery: CategoriesQuery) {
    super();
    this.categoriesQuery.execute().subscribe((v) => {
      this.mainCategories = v.subCategories;
    });
  }

  ngOnInit() {
  }

  selectMainCategory(selectedCategoryName: string) {
    console.log(selectedCategoryName);
    if (!selectedCategoryName) {
      this.selectedSubCategory = null;
      this.selectedSubCategory2 = null;
      this.selectedMainCategory = null;
      return;
    }
    this.selectedMainCategory =
      this.mainCategories.filter(c => c.categoryName === selectedCategoryName)[0];
    this.subCategories = this.selectedMainCategory.subCategories;
    this.selectedSubCategory = null;
    this.selectedSubCategory2 = null;
    this.subCategories2 = [];
    console.log(this.subCategories);
  }

  selectSubCategory(selectedCategoryName: string) {
    if (!selectedCategoryName) {
      this.selectedSubCategory = null;
      this.selectedSubCategory2 = null;
      return;
    }
    this.selectedSubCategory
      = this.subCategories.filter(c => c.categoryName === selectedCategoryName)[0];
    this.subCategories2 = this.selectedSubCategory.subCategories;
  }

  selectSubCategory2(selectedCategoryName: string) {
    if (!selectedCategoryName) {
      return;
    }
    this.selectedSubCategory2 = this.subCategories2.filter(c => c.categoryName === selectedCategoryName)[0];
  }

  onOkClick() {
    if (this.selectedMainCategory && this.selectedSubCategory && this.selectedSubCategory2) {
      this.completeStep(new CategorySelectStep(this.selectedMainCategory, this.selectedSubCategory, this.selectedSubCategory2));
    }

  }

}
