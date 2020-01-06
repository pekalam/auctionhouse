import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CategoryTreeNode } from '../models/CategoryTreeNode';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CategoriesQuery {
  constructor(private httpClient: HttpClient) {

  }

  execute(): Observable<CategoryTreeNode> {
    const url = `${environment.API_URL}/api/categories`;
    return this.httpClient.get<CategoryTreeNode>(url, {});
  }

  getMainCategoryTree(categoryName: string): Observable<CategoryTreeNode> {
    return this.execute().pipe(
      map(categories => categories.subCategories.find(n => n.categoryName === categoryName))
    );
  }

  getSubcategoryTree(categoryName: string, subCategoryName: string) : Observable<CategoryTreeNode>{
    return this.execute().pipe(
      map(categories => categories.subCategories.find(n => n.categoryName === categoryName)
        .subCategories.find(n => n.categoryName === subCategoryName))
    );
  }
}
