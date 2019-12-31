import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Auction, AuctionListModel } from '../../../core/models/Auctions';
import { Category } from '../../../core/models/Category';
import { AuctionsQuery, ConditionQuery, AuctionFilters } from '../../../core/queries/AuctionsQuery';
import { CategoriesQuery } from '../../../core/queries/CategoriesQuery';
import { CategoryTreeNode } from '../../../core/models/CategoryTreeNode';
import { Condition } from 'src/app/core/models/Product';
import { FilterCategory } from '../../components/auction-filters/auction-filters.component';
import { StartAuctionCreateSessionCommand } from '../../../core/commands/auction/StartAuctionCreateSessionCommand';
import { AuctionsByTagQuery } from '../../../core/queries/AuctionsByTagQuery';
import { CommonTagsQuery } from '../../../core/queries/CommonTagsQuery';
import { filter } from 'rxjs/operators';
import { PageEvent } from '@angular/material';
import { UserBid } from 'src/app/core/models/UserBid';

@Component({
  selector: 'app-auctions-page',
  templateUrl: './auctions-page.component.html',
  styleUrls: ['./auctions-page.component.scss']
})
export class AuctionsPageComponent implements OnInit {
  auctions: AuctionListModel[];
  filterCategories: FilterCategory;
  selectedFilterCategoryValue: string;
  total: number = 0;

  currentPage = 0;
  private currentFilters: AuctionFilters;
  private currentCategory: Category;
  private tag = '';

  constructor(private activatedRoute: ActivatedRoute, private auctionsByCategoryQuery: AuctionsQuery,
    private categoriesQuery: CategoriesQuery, private auctionsByTagQuery: AuctionsByTagQuery,
    private commonTagsQuery: CommonTagsQuery, private router: Router) {
  }

  private createFilterCategoriesByCategory(mainCategoryName: string, treeNode: CategoryTreeNode) {
    const subCategories2: FilterCategory[] = treeNode.subCategories.map<FilterCategory>((t) => {
      const filterCat: FilterCategory = {
        link: ['/auctions', mainCategoryName, treeNode.categoryName, t.categoryName],
        value: t.categoryName,
        children: [],
        queryParams: null
      };
      return filterCat;
    });
    console.log(treeNode);

    this.filterCategories = {
      link: ['/category', mainCategoryName],
      value: mainCategoryName,
      children: subCategories2,
      queryParams: null
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
      .subscribe((v) => this.createFilterCategoriesByCategory(mainCategoryName, v));

    this.selectedFilterCategoryValue = subCategory2Name;
    this.fetchAuctions();
  }

  private createFilterCategoryByTag(tag: string) {
    this.commonTagsQuery.execute(tag).subscribe((result) => {
      if (!result) {
        this.router.navigateByUrl('/error');
      }
      this.filterCategories = {
        link: null,
        value: "Tags",
        queryParams: null,
        children: result.withTags.map<FilterCategory>((t) => {
          let filterCat: FilterCategory = {
            link: ['/auctions'],
            value: `${t.tag} (${t.times})`,
            children: [],
            queryParams: { tag: t.tag }
          };
          return filterCat;
        })
      }
      this.tag = `${result.withTags[0].tag}`;
      this.fetchAuctions();
    });
  }

  applyFilters(filters: AuctionFilters) {
    this.currentFilters = filters;
    this.fetchAuctions();
  }

  fetchAuctions() {
    if (this.tag.length === 0) {
      this.auctionsByCategoryQuery
        .execute(this.currentPage, this.currentCategory, this.currentFilters)
        .subscribe(v => {
          this.auctions = v.auctions;
          this.total = v.total;
        }, () => this.router.navigateByUrl('/error'));
    } else {
      this.auctionsByTagQuery
        .execute(this.currentPage, this.tag, this.currentFilters)
        .subscribe(v => {
          this.auctions = v.auctions;
          this.total = v.total;
        }, () => this.router.navigateByUrl('/error'));
    }
  }

  ngOnInit() {
    this.activatedRoute.queryParams.subscribe((q) => {
      if (q.tag) {
        this.createFilterCategoryByTag(q.tag);
      } else {
        this.activatedRoute.params.subscribe(p => this.constructCategory(p.mainCategory,
          p.subCategory, p.subCategory2));
      }
    });

  }

  onGotoClick(e){
    this.currentPage = e.target.value - 1;
    e.target.value = '';
    this.fetchAuctions();
  }

  onPageChange(ev: PageEvent) {
    this.currentPage = ev.pageIndex;
    this.fetchAuctions();
  }

}
