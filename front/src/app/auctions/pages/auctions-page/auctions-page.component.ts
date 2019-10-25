import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Auction, AuctionListModel } from '../../../core/models/Auctions';
import { Category } from '../../../core/models/Category';
import { AuctionsQuery, ConditionQuery, AuctionFilters } from '../../../core/queries/AuctionsQuery';
import { CategoriesQuery } from '../../../core/queries/CategoriesQuery';
import { CategoryTreeNode } from '../../../core/models/CategoryTreeNode';
import { Condition } from 'src/app/core/models/Product';

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
    const cat: Category = { name: mainCategoryName, subCategory: null };
    cat.subCategory = { name: subCategoryName, subCategory: null };
    if (subCategory2Name) {
      cat.subCategory.subCategory = { name: subCategory2Name, subCategory: null };
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

  applyFilters(filters: AuctionFilters){
    console.log("applied");
    console.log(filters);

    this.fetchAuctions(0, filters);
  }

  fetchAuctions(page: number, filters?: AuctionFilters) {
    this.auctionsQuery
      .execute(page, this.currentCategory, filters)
      .subscribe(v => this.auctions = v);
  }

  ngOnInit() {
    this.activatedRoute.params.subscribe(p => this.constructCategory(p.mainCategory,
      p.subCategory, p.subCategory2));
  }

}
