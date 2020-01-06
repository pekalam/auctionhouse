import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CategoryTreeNode } from '../models/CategoryTreeNode';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface TagOccurence {
  tag: string;
  times: number;
}

export interface CommonTagsQueryResult {
  tag: string;
  withTags: TagOccurence[];
}

@Injectable({
  providedIn: 'root'
})
export class CommonTagsQuery {
  constructor(private httpClient: HttpClient) {
  }

  execute(tag: string): Observable<CommonTagsQueryResult> {
    const url = `${environment.API_URL}/api/commonTags?tag=${tag}`;
    return this.httpClient.get<CommonTagsQueryResult>(url, {});
  }
}
