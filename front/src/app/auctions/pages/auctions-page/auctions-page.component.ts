import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Auction, AuctionListModel } from '../../../core/models/Auctions';
import { Category } from '../../../core/models/Category';
import { AuctionsQuery, ConditionQuery, AuctionFilters } from '../../../core/queries/AuctionsQuery';
import { CategoriesQuery } from '../../../core/queries/CategoriesQuery';
import { CategoryTreeNode } from '../../../core/models/CategoryTreeNode';
import { Condition } from 'src/app/core/models/Product';
import { FilterCategory } from '../../components/auction-filters/auction-filters.component';

@Component({
  selector: 'app-auctions-page',
  templateUrl: './auctions-page.component.html',
  styleUrls: ['./auctions-page.component.scss']
})
export class AuctionsPageComponent implements OnInit {
  auctions: AuctionListModel[];
  filterCategories: FilterCategory;
  selectedFilterCategoryValue: string;
  private currentCategory: Category;


  constructor(private activatedRoute: ActivatedRoute, private auctionsQuery: AuctionsQuery,
              private categoriesQuery: CategoriesQuery) {
  }

  private createFilterCategories(mainCategoryName: string, treeNode: CategoryTreeNode) {
    const subCategories2: FilterCategory[] = treeNode.subCategories.map<FilterCategory>((t) => {
      const filterCat: FilterCategory = {
        link: ['/auctions', mainCategoryName, treeNode.categoryName, t.categoryName],
        value: t.categoryName,
        children: []
      };
      return filterCat;
    });
    console.log(treeNode);

    this.filterCategories = {
      link: ['/category', mainCategoryName],
      value: mainCategoryName,
      children: subCategories2
    };
    console.log(this.filterCategories
    );
  }

  private constructCategory(mainCategoryName: string, subCategoryName: string, subCategory2Name: string) {
    const cat: Category = {
      name: mainCategoryName,
      subCategory: {
        name: subCategoryName,
        subCategory: {
          name: subCategory2Name,
          subCategory: null
        }
      }
    };
    this.currentCategory = cat;
    this.categoriesQuery
      .getSubcategoryTree(mainCategoryName, subCategoryName)
      .subscribe((v) => this.createFilterCategories(mainCategoryName, v));

    this.selectedFilterCategoryValue = subCategory2Name;
    this.fetchAuctions(0);
  }

  applyFilters(filters: AuctionFilters) {
    console.log('applied');
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
