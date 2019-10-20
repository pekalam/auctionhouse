import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Auction, AuctionListModel } from '../../../core/models/Auctions';
import { Category } from '../../../core/models/Category';
import { AuctionsQuery, Condition } from '../../../core/queries/AuctionsQuery';
import { CategoriesQuery } from '../../../core/queries/CategoriesQuery';
import { CategoryTreeNode } from '../../../core/models/CategoryTreeNode';

@Component({
  selector: 'app-auctions-page',
  templateUrl: './auctions-page.component.html',
  styleUrls: ['./auctions-page.component.scss']
})
export class AuctionsPageComponent implements OnInit {
  auctions: AuctionListModel[];
  subCategoryTree: CategoryTreeNode;
  private currentCategory: Category;


  constructor(private activatedRoute: ActivatedRoute, private auctionsQuery: AuctionsQuery,
    private categoriesQuery: CategoriesQuery) {

  }

  private constructCategory(mainCategoryName: string, subCategoryName: string, subCategory2Name?: string) {
    const cat: Category = { categoryName: mainCategoryName, subCategory: null };
    cat.subCategory = { categoryName: subCategoryName, subCategory: null };
    if (subCategory2Name) {
      cat.subCategory.subCategory = { categoryName: subCategory2Name, subCategory: null };
    }
    this.currentCategory = cat;
    this.categoriesQuery.getSubcategoryTree(mainCategoryName, subCategoryName).subscribe((v) => {
      this.subCategoryTree = {
        categoryName: mainCategoryName,
        subCategories: [v]
      };
    });
    this.fetchAuctions(0);
  }

  fetchAuctions(page: number) {
    this.auctionsQuery
      .execute(page, this.currentCategory, Condition.all)
      .subscribe(v => this.auctions = v);
  }

  ngOnInit() {
    this.activatedRoute.params.subscribe(p => this.constructCategory(p.mainCategory,
      p.subCategory, p.subCategory2));
  }

}
