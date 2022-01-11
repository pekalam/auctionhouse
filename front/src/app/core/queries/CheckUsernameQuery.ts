import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CategoryTreeNode } from '../models/CategoryTreeNode';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface CheckUsernameQueryResult{
  username: string;
  exist: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class CheckUsernameQuery {
  constructor(private httpClient: HttpClient) {

  }

  execute(username: string): Observable<CheckUsernameQueryResult> {
    const url = `${environment.API_URL}/api/q/checkUsername?username=${username}`;
    return this.httpClient.get<CheckUsernameQueryResult>(url, {});
  }
}
