import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { tap, catchError, switchMap } from 'rxjs/operators';
import { LoadingService } from '../services/LoadingService';



@Injectable({
  providedIn: 'root'
})
export class QueryHelper {

  constructor(private loadingService: LoadingService) { }

  pipeLoading(queryRequest: Observable<any>): Observable<any> {
    this.loadingService.setLoading(true);
    return queryRequest.pipe(
      catchError((err) => {
        this.loadingService.setLoading(false);
        throw err;
      }),
      switchMap((result) => {
        this.loadingService.setLoading(false);
        return of(result);
      })
    )
  }
}
